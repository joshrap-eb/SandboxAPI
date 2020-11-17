using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SandboxAPI.Models;
using SandboxAPI.Services;

namespace SandboxAPI.Controllers
{
	[ApiController]
	[ApiVersion( "1" )]
	[Route( "api/v{version:apiVersion}/[controller]" )]
	public class MovieController
	{
		private readonly ILogger<MovieController> _logger;
		private readonly IHttpContextAccessor _context;
		private readonly IMovieService _movieService;

		public MovieController( ILogger<MovieController> logger, IMovieService movieService,
			IHttpContextAccessor httpContextAccessor )
		{
			_movieService = movieService;
			_logger = logger;
			_context = httpContextAccessor;
		}

		/// <summary>
		/// Returns a movie.
		/// </summary>
		/// <returns>Movie given the passed in name</returns>
		/// <response code="200">Movie found</response>
		/// <response code="404">Movie is not found</response>
		/// <response code="500">Oopsie whoopsie! The code monkeys can't get your movie right now</response>
		[ProducesResponseType( typeof( Movie ), 200 )]
		[ProducesResponseType( 400 )]
		[ProducesResponseType( 500 )]
		[HttpGet( "{movieName}" )]
		public ActionResult<Movie> Get( string movieName )
		{
			var token = _context.HttpContext.Request
				.Headers["HeaderAuthorization"]; // todo use this for authorization purposes?

			if ( string.IsNullOrWhiteSpace( movieName ) )
			{
				return new BadRequestObjectResult( "Movie name cannot be empty." );
			}

			var movie = _movieService.Get( movieName );
			if ( movie == null )
			{
				return new NotFoundObjectResult( "Movie Not Found" );
			}

			movie.Director = token.ToString( ); // just because too lazy to configure logs rn
			return movie;
		}

		/// <summary>
		/// Creates a new movie if it doesn't already exist.
		/// </summary>
		/// <response code="200">Movie created</response>
		/// <response code="400">Movie could not be added</response>
		/// <response code="500">Can't create your movie right now</response>
		[ProducesResponseType( 200 )]
		[ProducesResponseType( 400 )]
		[ProducesResponseType( 500 )]
		[HttpPut]
		public ActionResult Create( Movie movie )
		{
			if ( movie == null )
			{
				return new BadRequestResult( );
			}

			var success = _movieService.Create( movie );
			if ( success )
			{
				return new OkResult( );
			}

			return new BadRequestResult( );
		}

		/// <summary>
		/// Returns all movies in the database
		/// </summary>
		/// <response code="200">Movies returned</response>
		/// <response code="400">Movies could not be returned</response>
		/// <response code="500">Can't return all movies right now</response>
		[ProducesResponseType( typeof( IEnumerable<Movie> ), 200 )]
		[ProducesResponseType( 400 )]
		[ProducesResponseType( 500 )]
		[HttpGet]
		public ActionResult GetAll( )
		{
			var movies = _movieService.GetAll( );
			return new OkObjectResult( movies );
		}
	}
}