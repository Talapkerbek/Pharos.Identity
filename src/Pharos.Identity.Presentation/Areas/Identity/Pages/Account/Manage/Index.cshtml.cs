// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Pharos.Identity.Application.Features.CompleteAvatarUpdate;
using Pharos.Identity.Application.Features.GetPresignedUrlForAvatarLogo;
using Pharos.Identity.Infra;
using Pharos.Identity.Infra.Data;
using Pharos.Media.Contracts;
using Wolverine;

namespace Pharos.Identity.Presentation.Areas.Identity.Pages.Account.Manage
{
    [RequestSizeLimit(100_000_000)]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IMessageBus _messageBus;
        private readonly IConfiguration _configuration;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IStringLocalizer<SharedResource>  localizer,
            IMessageBus messageBus,
            IConfiguration configuration
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _localizer = localizer;
            _messageBus = messageBus;
            _configuration = configuration;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        ///
        /// 
        [BindProperty]
        public IFormFile? AvatarFile { get; set; } // <-- отдельно, не внутри Input
        
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "FirstName")]
            public string FirstName { get; set; }
            
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "LastName")]
            public string LastName { get; set; }
            
            public string InitialAvatarPath { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);

            Username = userName;

            Input = new InputModel
            {
                FirstName  = user.FirstName,
                LastName = user.LastName,
                InitialAvatarPath = $"{_configuration["CloudflareCdnUrl"]}/{user.AvatarPath}"
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }
            
            if (Input.FirstName != user.FirstName)
            {
                user.FirstName = Input.FirstName;
            }

            if (Input.LastName != user.LastName)
            {
                user.LastName = Input.LastName;
            }
            
            await _userManager.UpdateAsync(user);

            Console.WriteLine(AvatarFile == null);
            
            Console.WriteLine(AvatarFile?.Length);
            
            if (AvatarFile is { Length: > 0 })
            {
                var checksum = await ComputeChecksumAsync(AvatarFile);
                var presigned = await _messageBus.InvokeAsync<GetPresignedUrlResponse>(
                    new GetPresignedUrlForAvatarLogoCommand(
                        UserId: Guid.Parse(user.Id),
                        Size: AvatarFile.Length,
                        Checksum: checksum,
                        ContentType: AvatarFile.ContentType,
                        UploaderId: Guid.Parse(user.Id)
                    )
                );

                if (presigned is null)
                {
                    ModelState.AddModelError("", "Failed to get upload URL.");
                    return Page();
                }
                
                if (!presigned.IsBlobExists)
                {
                    using var http = new HttpClient();
                    
                    await using var stream = AvatarFile.OpenReadStream();

                    var content = new StreamContent(stream);
                    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                        AvatarFile.ContentType ?? "image/png"
                    );

                    var response = await http.PutAsync(presigned.PresignedUrl, content);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorText = await response.Content.ReadAsStringAsync();
                        throw new Exception($"Upload failed: {(int)response.StatusCode} {errorText}");
                    }
                }

                await _messageBus.InvokeAsync(
                    new CompleteAvatarUpdateCommand(
                        Guid.Parse(user.Id),
                        presigned.UploadId
                    )
                );
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = _localizer["ProfileUpdated"];
            return RedirectToPage();
        }
        
        private static async Task<string> ComputeChecksumAsync(IFormFile file)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            using var stream = file.OpenReadStream();
            var hash = await sha256.ComputeHashAsync(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
