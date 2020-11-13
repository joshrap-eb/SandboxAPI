using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SandboxAPI.Models
{
	public class Movie
	{
		/// <summary>
		/// Name of the movie.
		/// </summary>
		/// <example>Inception</example>
		[Required]
		public string Name { get; set; }

		/// <summary>
		/// Director of the movie. For movies with multiple, this will be comma delimited
		/// </summary>
		public string Director { get; set; }

		/// <summary>
		/// Genres of the movie.
		/// </summary>
		public IEnumerable<string> Genre { get; set; }

		/// <summary>
		/// Personal rating of the movie (1-5)
		/// </summary>
		public int? Rating { get; set; }
	}
}