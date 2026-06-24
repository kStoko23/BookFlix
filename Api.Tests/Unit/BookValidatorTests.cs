using Api.Entities;
using Api.Features.Auth;
using Api.Features.Books;

namespace Api.Tests.Unit;

public class BookValidatorTests
{
    private readonly BookValidator _validator = new();

    [Fact]
    public void ValidateTitle_WhenEmpty_ShouldReturnError()
    {
        var request = new CreateBookRequest(
            "",
            "Author",
            "Desc",
            null,
            "1234567890",
            100,
            3);

        var errors = _validator.ValidateCreateOrUpdateRequest(request);

        Assert.Contains("title", errors.Keys);
        Assert.Contains("Title is required", errors["title"]);
    }

    [Fact]
    public void ValidateTitle_WhenTooLong_ShouldReturnError()
    {
        var request = new CreateBookRequest(
            new string('a', 301),
            "Author",
            "Desc",
            null,
            "1234567890",
            100,
            3);

        var errors = _validator.ValidateCreateOrUpdateRequest(request);

        Assert.Contains("title", errors.Keys);
        Assert.Contains("Title is too long", errors["title"]);
    }

    [Fact]
    public void ValidateAuthor_WhenEmpty_ShouldReturnError()
    {
        var request = new CreateBookRequest(
            "Title",
            "",
            "Desc",
            null,
            "1234567890",
            100,
            3);

        var errors = _validator.ValidateCreateOrUpdateRequest(request);

        Assert.Contains("author", errors.Keys);
        Assert.Contains("Author is required", errors["author"]);
    }

    [Fact]
    public void ValidateAuthor_WhenTooLong_ShouldReturnError()
    {
        var request = new CreateBookRequest(
            "Title",
            new string('a', 201),
            "Desc",
            null,
            "1234567890",
            100,
            3);

        var errors = _validator.ValidateCreateOrUpdateRequest(request);

        Assert.Contains("author", errors.Keys);
        Assert.Contains("Author is too long", errors["author"]);
    }

    [Fact]
    public void ValidateDescription_WhenOnlyWhitespace_ShouldReturnError()
    {
        var request = new CreateBookRequest(
            "Title",
            "Author",
            "   ",
            null,
            "1234567890",
            100,
            3);

        var errors = _validator.ValidateCreateOrUpdateRequest(request);

        Assert.Contains("description", errors.Keys);
        Assert.Contains("Description cannot be only whitespace", errors["description"]);
    }

    [Fact]
    public void ValidateDescription_WhenTooLong_ShouldReturnError()
    {
        var request = new CreateBookRequest(
            "Title",
            "Author",
            new string('a', 1201),
            null,
            "1234567890",
            100,
            3);

        var errors = _validator.ValidateCreateOrUpdateRequest(request);

        Assert.Contains("description", errors.Keys);
        Assert.Contains("Description is too long", errors["description"]);
    }

    [Fact]
    public void ValidateDescription_WhenNull_ShouldNotReturnError()
    {
        var request = new CreateBookRequest(
            "Title",
            "Author",
            null,
            null,
            "1234567890",
            100,
            3);

        var errors = _validator.ValidateCreateOrUpdateRequest(request);

        Assert.Empty(errors);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void ValidateIsbn_WhenEmpty_ShouldReturnError(string isbn)
    {
        var request = new CreateBookRequest(
            "Title",
            "Author",
            "Desc",
            null,
            isbn,
            100,
            3);

        var errors = _validator.ValidateCreateOrUpdateRequest(request);

        Assert.Contains("isbn", errors.Keys);
        Assert.Contains("ISBN is required", errors["isbn"]);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("invalid-isbn")]
    public void ValidateIsbn_WhenInvalid_ShouldReturnError(string isbn)
    {
        var request = new CreateBookRequest(
            "Title",
            "Author",
            "Desc",
            null,
            isbn,
            100,
            3);

        var errors = _validator.ValidateCreateOrUpdateRequest(request);

        Assert.Contains("isbn", errors.Keys);
        Assert.Contains("Given ISBN is not valid ISBN number", errors["isbn"]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void ValidatePages_WhenInvalid_ShouldReturnError(int pages)
    {
        var request = new CreateBookRequest(
            "Title",
            "Author",
            "Desc",
            null,
            "1234567890",
            pages,
            3);

        var errors = _validator.ValidateCreateOrUpdateRequest(request);

        Assert.Contains("pages", errors.Keys);
        Assert.Contains("Number of pages is required", errors["pages"]);
    }

    [Fact]
    public void ValidatePages_WhenTooLarge_ShouldReturnError()
    {
        var request = new CreateBookRequest(
            "Title",
            "Author",
            "Desc",
            null,
            "1234567890",
            10001,
            3);

        var errors = _validator.ValidateCreateOrUpdateRequest(request);

        Assert.Contains("pages", errors.Keys);
        Assert.Contains("Number of pages is too large", errors["pages"]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ValidateRating_WhenInvalid_ShouldReturnError(int rating)
    {
        var request = new CreateBookRequest(
            "Title",
            "Author",
            "Desc",
            null,
            "1234567890",
            100,
            rating);

        var errors = _validator.ValidateCreateOrUpdateRequest(request);

        Assert.Contains("rating", errors.Keys);
        Assert.Contains("Number of rating is required", errors["rating"]);
    }

    [Fact]
    public void ValidateRating_WhenTooHigh_ShouldReturnError()
    {
        var request = new CreateBookRequest(
            "Title",
            "Author",
            "Desc",
            null,
            "1234567890",
            100,
            10);

        var errors = _validator.ValidateCreateOrUpdateRequest(request);

        Assert.Contains("rating", errors.Keys);
        Assert.Contains("Rating can only be between 1 and 5", errors["rating"]);
    }

    [Fact]
    public void ValidateCategory_WhenInvalidEnumValue_ShouldReturnError()
    {
        var request = new CreateBookRequest(
            "Title",
            "Author",
            "Desc",
            (BookCategory)999,
            "1234567890",
            100,
            3);

        var errors = _validator.ValidateCreateOrUpdateRequest(request);

        Assert.Contains("category", errors.Keys);
        Assert.Contains("Category is invalid", errors["category"]);
    }
    
    [Fact]
    public void ValidateMultipleViolations_ShouldAccumulateErrorsPerField()
    {
        var validator = new BookValidator();

        var request = new CreateBookRequest(
            "",
            "",
            "   ",
            null,
            "invalid",
            0,
            10);

        var errors = validator.ValidateCreateOrUpdateRequest(request);

        Assert.Contains("title", errors.Keys);
        Assert.Contains("Title is required", errors["title"]);

        Assert.Contains("author", errors.Keys);
        Assert.Contains("Author is required", errors["author"]);

        Assert.Contains("isbn", errors.Keys);
        Assert.Contains("Given ISBN is not valid ISBN number", errors["isbn"]);

        Assert.Contains("pages", errors.Keys);
        Assert.Contains("Number of pages is required", errors["pages"]);

        Assert.Contains("rating", errors.Keys);
        Assert.Contains("Rating can only be between 1 and 5", errors["rating"]);
    }

    [Theory]
    [InlineData("123-123-123-0")]
    [InlineData("1231231230")]
    [InlineData("12---312312---30")]
    public void ValidateCreateOrUpdateRequest_WhenValid_ShouldReturnNoErrors(string isbn)
    {
        var request = new CreateBookRequest(
            "Clean Code",
            "Robert Martin",
            "Good book",
            null,
            isbn,
            300,
            4);

        var errors = _validator.ValidateCreateOrUpdateRequest(request);

        Assert.Empty(errors);
    }
}