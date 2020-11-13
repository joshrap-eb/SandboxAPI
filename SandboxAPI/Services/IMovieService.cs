using System.Collections.Generic;
using SandboxAPI.Models;

namespace SandboxAPI.Services
{
	public interface IMovieService
	{
		public Movie Get( string name );

		public bool Create( Movie movie );

		public IEnumerable<Movie> GetAll( );
	}
}