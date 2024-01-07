using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using Tippser.Core.Entities;
using Tippser.Core.Interfaces;
using Tippser.Presentation.Client.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Tippser.Core;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Tippser.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BaseApiController<TClass> : ControllerBase
        where TClass : ControllerBase
    {
        
    }

    public class CommonController : BaseApiController<CommonController>
    {
        [HttpGet(nameof(SetCulture))]
        public ActionResult<string> SetCulture(string returnUrl = "/")
        {
            var currentCulture = CultureInfo.CurrentCulture.Name;
            var newCulture = currentCulture;
            switch (currentCulture)
            {
                case "sv-SE":
                    newCulture = "en-US";
                    break;
                case "en-US":
                    newCulture = "en-GB";
                    break;
                case "en-GB":
                    newCulture = "sv-SE";
                    break;
                default:
                    break;
            }

            if (!string.IsNullOrEmpty(currentCulture))
            {
                var cultureInfo = new CultureInfo(newCulture);
                Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(cultureInfo)),
                    new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
                );
            }

            return LocalRedirect(returnUrl);

        }
    }

    public class AccountController(IUserService userService, ISignInService signInService, IConfiguration configuration) : BaseApiController<AccountController>
    {
        private readonly IUserService _userService = userService;
        private readonly ISignInService _signInService = signInService;
        private readonly IConfiguration _configuration = configuration;


        [HttpGet(nameof(SignOut))]
        public new async Task SignOut()
        {
            try
            {
                await _signInService.SignOut(HttpContext);
            }
            catch (Exception)
            {

            }    
        }

        [HttpPost(nameof(SignIn))]
        public async Task<ActionResult<Client.Models.SignInResult>> SignIn(SignInModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Bad model");
            }

            var cancellationToken = new CancellationToken();

            var user = await _userService.FindByEmailAsync(model.Email.ToUpper(), cancellationToken);
            if (user != null && (await _signInService.CheckPasswordSignInAsync(user, model.Password)).Succeeded)
            {
                var authClaims = new[]
                {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    expires: DateTime.Now.AddHours(3),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
            }
            return Unauthorized();
        }

        [HttpPost(nameof(Create))]
        public async Task<ActionResult<IdentityResult>> Create(CreateUserModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var cancellationToken = new CancellationToken();
            Person item = new Person(model.Name, model.Email, model.Confirm, Constants.SystemUserId);  
            
            var response = await _userService.CreateAsync(item, cancellationToken);
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            response = await _userService.AddToRoleAsync(item, Constants.Roles.User);
            if (!response.Succeeded)
            {
                return BadRequest(response);

            }

            return Ok(response);
        }

        [HttpPost(nameof(ForgotPassword))]
        public IActionResult ForgotPassword(Person person)
        {
           //Forgot password method
            return Ok();
        }
    }
    
    [Authorize]
    public class BetsController : BaseApiController<BetsController>
    {
        [HttpGet(nameof(Get))]
        public ActionResult<IEnumerable<Bet>> Get()
        {
            // Replace with actual logic to retrieve bets
            return Ok(new List<Bet>());
        }
    }

    [Authorize]
    public class CompetitionsController(ILogger<CompetitionsController> logger, IWebHostEnvironment environment, IDbService dbService, IMatchDataService matchDataService) : BaseApiController<CompetitionsController>
    {
        private readonly ILogger _logger = logger;
        private readonly IWebHostEnvironment _environment = environment;
        private readonly IDbService _dbService = dbService;
        private readonly IMatchDataService _matchDataService = matchDataService;

        [HttpGet(nameof(GetStandings))]
        public async Task<ActionResult<StandingsViewModel>> GetStandings()
        {
            try
            {
                var competitions = await _dbService.Read<Competition>();
                var competitionData = _matchDataService.CollateCompetitionData(competitions!);
                var model = new StandingsViewModel(competitionData?.FirstOrDefault()!)
                {
                    User = new Person()
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in {nameof(GetStandings)}. {ex.Message}");
                if (!_environment.IsDevelopment())
                {
                    throw;
                }
            }

            return BadRequest();
        }
    }

    [Authorize]
    public class MatchesController : BaseApiController<MatchesController>
    {
        [HttpGet(nameof(Get))]
        public ActionResult<IEnumerable<Match>> Get()
        {
            // Replace with actual logic to retrieve matches
            return Ok(new List<Match>());
        }
    }

    [Authorize]
    public class ManageController : BaseApiController<ManageController>
    {
        [HttpGet(nameof(Get))]
        public ActionResult<IEnumerable<Match>> Get()
        {
            return Ok();
        }
    }
}
