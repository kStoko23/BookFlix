using System.Text.RegularExpressions;
using Api.Entities;
using Api.Features.Books;

namespace Api.Features.Books;

public class BookValidator
{
     private readonly Dictionary<string, string[]> _errors = new();

     public bool HasErrors => _errors.Count > 0;
     private Dictionary<string, string[]> Errors => _errors;
     
     private static readonly Regex IsbnRegex = new(
          // Source - https://stackoverflow.com/a/62951500
          // Posted by Thor Hovden
          // Retrieved 2026-06-20, License - CC BY-SA 3.0
          

          @"^(?=(?:\D*\d){10}(?:(?:\D*\d){3})?$)[\d-]+$",
          RegexOptions.Compiled
     );

     private void AddError(string field, string message)
     {
          if (_errors.TryGetValue(field, out var existing))
          {
               _errors[field] = existing.Append(message).ToArray();
          }
          else
          {
               _errors[field] = [message];
          }
     }
     private BookValidator ValidateTitle(string title)
     {
          if (string.IsNullOrWhiteSpace(title))
          {
               AddError("title", "Title is required");
               return this;
          }
          if(title.Length > 300)
               AddError("title", "Title is too long");
          
          return this;
     }
     private BookValidator ValidateAuthor(string author)
     {
          if (string.IsNullOrWhiteSpace(author))
          {
               AddError("author", "Author is required");
               return this;
          }
          if(author.Length > 200)
               AddError("author", "Author is too long");
          
          return this;
     }
     private BookValidator ValidateDescription(string? description)
     {
          if (description is null)
               return this;

          if (description.All(char.IsWhiteSpace))
          {
               AddError("description", "Description cannot be only whitespace");
               return this;
          }

          if (description.Length > 1200)
               AddError("description", "Description is too long");

          return this;
     }
     private BookValidator ValidateIsbn(string isbn)
     {
          if (string.IsNullOrWhiteSpace(isbn))
          {
               AddError("isbn", "ISBN is required");
               return this;
          }
          if (!IsbnRegex.IsMatch(isbn))
               AddError("isbn", "Given ISBN is not valid ISBN number");
          
          return this;
     }

     private BookValidator ValidatePages(int pages)
     {
          if (pages <= 0 )
          {
               AddError("pages", "Number of pages is required");
               return this;
          }
          if(pages > 10000)
               AddError("pages", "Number of pages is too large");
          
          return this;
     }

     private BookValidator ValidateRating(int rating)
     {
          if (rating <= 0)
          {
               AddError("rating", "Number of rating is required");
               return this;
          }
          if( rating > 5)
               AddError("rating", "Rating can only be between 1 and 5");
          
          return this;
     }
     private BookValidator ValidateCategory(BookCategory? category)
     {
          if(category.HasValue && !Enum.IsDefined(typeof(BookCategory), category))
               AddError("category", "Category is invalid");
          
          return this;
     }
     
     public Dictionary<string, string[]> ValidateCreateOrUpdateRequest(CreateBookRequest request)
     {
          ValidateTitle(request.Title);
          ValidateAuthor(request.Author);
          ValidateDescription(request.Description);
          ValidateIsbn(request.Isbn);
          ValidatePages(request.Pages);
          ValidateRating(request.Rating);
          ValidateCategory(request.Category);
          return Errors;
     }
}