using System.Collections.Generic;
using System.Linq;
using SandboxAPI.Models;

namespace SandboxAPI.Services
{
	public class MovieService : IMovieService
	{
		private static readonly List<Movie> DefaultMovies = new List<Movie>
		{
			new Movie
			{
				Name = "Interstellar",
				Director = "Christopher Nolan",
				Genre = new List<string> {"Sci-Fi", "Adventure"},
				Rating = 5
			},
			new Movie
			{
				Name = "Goodfellas",
				Director = "Martin Scorsese",
				Genre = new List<string> {"Drama", "Crime"},
				Rating = 4
			},
			new Movie
			{
				Name = "Knives Out",
				Director = "Rian Johnson",
				Genre = new List<string> {"Mystery", "Crime"},
				Rating = 5
			},
		};

		private readonly ICollection<Movie> _movies;

		public MovieService( )
		{
			_movies = new List<Movie>( DefaultMovies );
		}

		public Movie Get( string name )
		{
			return ( _movies.Where( x => x.Name == name ) ).FirstOrDefault( );
		}

		public bool Create( Movie movie )
		{
			if ( _movies.Any( x => x.Name == movie.Name ) ) return false;
			_movies.Add( movie );
			return true;
		}

		public IEnumerable<Movie> GetAll( )
		{
			return _movies;
		}
	}
}