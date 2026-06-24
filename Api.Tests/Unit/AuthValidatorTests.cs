using Api.Features.Auth;

namespace DefaultNamespace;

public class AuthValidatorTests
{
    private readonly AuthValidator _authValidator = new();
    
    [Theory]
    [InlineData("notanemail")]
    [InlineData("missingdomain@")]
    [InlineData("@onlydomain")]
    public void ValidateEmail_WhenInvalidEmail_ShouldReturnError(string email)
    {
        _authValidator.ValidateEmail(email);
        
        Assert.True(_authValidator.HasErrors);
        Assert.Contains("email", _authValidator.Errors.Keys);
        Assert.Contains("Email is not a valid email address", _authValidator.Errors["email"]);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("    ")]
    public void ValidateEmail_WhenEmptyOrWhitespace_ShouldReturnError(string email)
    {
        _authValidator.ValidateEmail(email);
        
        Assert.True(_authValidator.HasErrors);
        Assert.Contains("email", _authValidator.Errors.Keys);
        Assert.Contains("Email is required", _authValidator.Errors["email"]);
    }
    
    [Theory]
    [InlineData("test@test.com")]
    [InlineData("admin123@admin.pl")]
    public void ValidateEmail_IsValidEmail_ShouldNotReturnError(string email)
    {
        _authValidator.ValidateEmail(email);
        
        Assert.False(_authValidator.HasErrors);
        Assert.Empty(_authValidator.Errors);
    }
    
    [Fact]
    public void ValidateUsername_WhenEmpty_ShouldReturnError()
    {
        _authValidator.ValidateUsername(string.Empty);
        
        Assert.True(_authValidator.HasErrors);
        Assert.Contains("username", _authValidator.Errors.Keys);
        Assert.Contains("Username is required", _authValidator.Errors["username"]);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("ab")]
    public void ValidateUsername_WhenTooShort_ShouldReturnError(string username)
    {
        _authValidator.ValidateUsername(username);
        Assert.True(_authValidator.HasErrors);
        Assert.Contains("Username must be at least 3 characters", _authValidator.Errors["username"]);
    }

    [Theory]
    [InlineData("GH1L0WK7mZdWeX7irpxPr0yee88742kDhe0H2dlgZge4kBIsGFNcP3KZdvWciOkYe3D5LjO6i73zj2JrhiaabCyVaaxugCzGYLHvD")]
    [InlineData("GH1L0WK7mZdWeX7irpxPr0yee88742kDhe0H2dlgZge4kBIsGFNcP3KZdvWciOkYe3D5LjO6i73zj2JrhiaabCyVaaxugCzGYLHvDsez8J2je4T9IZEjHx9QrgbUd3KPGH1L0WK7mZdWeX7irpxPr0yee88742kDhe0H2dlgZge4kBIsGFNcP3KZdvWciOkYe3D5LjO6i73zj2JrhiaabCyVaaxugCzGYLHvDsez8J2je4T9IZEjHx9QrgbUd3KP")]
    public void ValidateUsername_WhenTooLong_ShouldReturnError(string username)
    {
        _authValidator.ValidateUsername(username);
        Assert.True(_authValidator.HasErrors);
        Assert.Contains("Username must be at most 100 characters", _authValidator.Errors["username"]);
    }

    [Theory]
    [InlineData("admin!")]
    [InlineData("admin user")]
    [InlineData("test@123")]
    public void ValidateUsername_WhenInvalidChars_ShouldReturnError(string username)
    {
        _authValidator.ValidateUsername(username);
        Assert.True(_authValidator.HasErrors);
        Assert.Contains("Username can only contain letters, digits and underscores", _authValidator.Errors["username"]);
    }
    
    [Theory]
    [InlineData("abc")]
    [InlineData("admin")]
    [InlineData("admin123")]
    [InlineData("admin123_")]
    [InlineData("123")]
    [InlineData("___")]
    [InlineData("GH1L0WK7mZdWeX7irpxPr0yee88742kDhe0H2dlgZge4kBIsGFNcP3KZdvWciOkYe3D5LjO6i73zj2JrhiaabCyVaaxugCzGYLHv")]
    public void ValidateUsername_IsValidUsername_ShouldNotReturnError(string username)
    {
        _authValidator.ValidateUsername(username);
        Assert.False(_authValidator.HasErrors);
        Assert.Empty(_authValidator.Errors);
    }
    
    [Fact]
    public void ValidatePassword_WhenEmpty_shouldReturnError()
    {
        _authValidator.ValidatePassword(string.Empty);
        
        Assert.True(_authValidator.HasErrors);
        Assert.Contains("password", _authValidator.Errors.Keys);
        Assert.Contains("Password is required", _authValidator.Errors["password"]);
    }

    [Fact]
    public void ValidatePassword_IsTooShort_ShouldReturnError()
    {
        _authValidator.ValidatePassword("pass");
        
        Assert.True(_authValidator.HasErrors);
        Assert.Contains("password", _authValidator.Errors.Keys);
        Assert.Contains("Password must be at least 8 characters", _authValidator.Errors["password"]);
    }
    
    [Fact]
    public void ValidatePassword_IsTooLong_ShouldReturnError()
    {
        _authValidator.ValidatePassword("GH1L0WK7mZdWeX7irpxPr0yee88742kDhe0H2dlgZge4kBIsGFNcP3KZdvWciOkYe3D5LjO6i73zj2JrhiaabCyVaaxugCzGYLHvDsez8J2je4T9IZEjHx9QrgbUd3KP");
        
        Assert.True(_authValidator.HasErrors);
        Assert.Contains("password", _authValidator.Errors.Keys);
        Assert.Contains("Password must be at most 64 characters", _authValidator.Errors["password"]);
    }
    
    [Fact]
    public void ValidatePassword_WhenMissingUppercase_ShouldReturnError()
    {
        _authValidator.ValidatePassword("password1");
        
        Assert.True(_authValidator.HasErrors);
        Assert.Contains("password", _authValidator.Errors.Keys);
        Assert.Contains("Password must contain at least one uppercase letter", _authValidator.Errors["password"]);
    }
    
    [Fact]
    public void ValidatePassword_WhenMissingLowercase_ShouldReturnError()
    {
        _authValidator.ValidatePassword("PASSWORD1");
        
        Assert.True(_authValidator.HasErrors);
        Assert.Contains("password", _authValidator.Errors.Keys);
        Assert.Contains("Password must contain at least one lowercase letter", _authValidator.Errors["password"]);
    }
    
    [Fact]
    public void ValidatePassword_WhenMissingDigit_ShouldReturnError()
    {
        _authValidator.ValidatePassword("Password");
        
        Assert.True(_authValidator.HasErrors);
        Assert.Contains("password", _authValidator.Errors.Keys);
        Assert.Contains("Password must contain at least one digit", _authValidator.Errors["password"]);
    }
    
    [Fact]
    public void ValidatePassword_WhenMultipleFieldsAreInvalid_ShouldReturnErrors()
    {
        _authValidator.ValidatePassword("_________");

        Assert.True(_authValidator.HasErrors);

        Assert.Contains("password", _authValidator.Errors.Keys);
        Assert.Contains("Password must contain at least one lowercase letter", _authValidator.Errors["password"]);
        Assert.Contains("Password must contain at least one uppercase letter", _authValidator.Errors["password"]);
        Assert.Contains("Password must contain at least one digit", _authValidator.Errors["password"]);
    }

    [Theory]
    [InlineData("Pas5word")]
    [InlineData("Password1")]
    [InlineData("aTqXutUS3617Tqb9MrwlivU3ZrdKrqIQ")]
    [InlineData("aTqXutUS3617Tqb9MrwlivU3ZrdKrqIQaTqXutUS3617Tqb9MrwlivU3ZrdKrqIQ")]
    public void ValidatePassword_IsValidPassword_ShouldNotReturnError(string password)
    {
        _authValidator.ValidatePassword(password);
        
        Assert.False(_authValidator.HasErrors);
        Assert.Empty(_authValidator.Errors);
    }
    
    [Fact]
    public void ValidateRegisterRequest_WhenAllFieldsAreInvalid_ShouldReturnErrors()
    {
        var request = new RegisterRequest
        {
            Email = "abc",
            Username = "a",
            Password = "pass"
        };

        var errors = _authValidator.ValidateRegisterRequest(request);

        Assert.True(_authValidator.HasErrors);

        Assert.Contains("email", errors.Keys);
        Assert.Contains("username", errors.Keys);
        Assert.Contains("password", errors.Keys);
    }
    
    [Fact]
    public void ValidateRegisterRequest_WhenEmailIsInvalid_ShouldReturnEmailError()
    {
        var request = new RegisterRequest
        {
            Email = "abc",
            Username = "admin123",
            Password = "Password1"
        };

        var errors = _authValidator.ValidateRegisterRequest(request);

        Assert.Single(errors);
        Assert.Contains("email", errors.Keys);
        Assert.Contains("Email is not a valid email address", errors["email"]);
    }
    
    [Fact]
    public void ValidateRegisterRequest_WhenUsernameIsInvalid_ShouldReturnUsernameError()
    {
        var request = new RegisterRequest
        {
            Email = "test@test.com",
            Username = "a",
            Password = "Password1"
        };

        var errors = _authValidator.ValidateRegisterRequest(request);

        Assert.Single(errors);
        Assert.Contains("username", errors.Keys);
        Assert.Contains("Username must be at least 3 characters", errors["username"]);
    }
    
    [Fact]
    public void ValidateRegisterRequest_WhenPasswordIsInvalid_ShouldReturnPasswordError()
    {
        var request = new RegisterRequest
        {
            Email = "test@test.com",
            Username = "admin123",
            Password = "password"
        };

        var errors = _authValidator.ValidateRegisterRequest(request);

        Assert.Single(errors);
        Assert.Contains("password", errors.Keys);
        Assert.Contains("Password must contain at least one uppercase letter", errors["password"]);
        Assert.Contains("Password must contain at least one digit", errors["password"]);
    }
    
    [Fact]
    public void ValidateRegisterRequest_WhenFieldsAreEmpty_ShouldReturnRequiredErrors()
    {
        var request = new RegisterRequest
        {
            Email = "",
            Username = "",
            Password = ""
        };

        var errors = _authValidator.ValidateRegisterRequest(request);

        Assert.Contains("Email is required", errors["email"]);
        Assert.Contains("Username is required", errors["username"]);
        Assert.Contains("Password is required", errors["password"]);
    }
    
    [Fact]
    public void ValidateRegisterRequest_WhenRequestIsValid_ShouldReturnNoErrors()
    {
        var request = new RegisterRequest
        {
            Email = "test@test.com",
            Username = "admin123",
            Password = "Password1"
        };

        var errors = _authValidator.ValidateRegisterRequest(request);

        Assert.False(_authValidator.HasErrors);
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateLoginRequest_WhenAllFieldsAreInvalid_ShouldReturnErrors()
    {
        var request = new LoginRequest
        {
            Email = "abc",
            Password = "pass"
        };

        var errors = _authValidator.ValidateLoginRequest(request);

        Assert.True(_authValidator.HasErrors);

        Assert.Contains("email", errors.Keys);
        Assert.Contains("password", errors.Keys);
    }
    
    [Fact]
    public void ValidateLoginRequest_WhenEmailIsInvalid_ShouldReturnError()
    {
        var request = new LoginRequest
        {
            Email = "abc",
            Password = "Password1"
        };

        var errors = _authValidator.ValidateLoginRequest(request);

        Assert.Single(errors);
        Assert.Contains("email", errors.Keys);
        Assert.Contains("Email is not a valid email address", errors["email"]);
    }
    
    [Fact]
    public void ValidateLoginRequest_WhenPasswordIsInvalid_ShouldReturnError()
    {
        var request = new LoginRequest
        {
            Email = "test@test.com",
            Password = "password"
        };

        var errors = _authValidator.ValidateLoginRequest(request);

        Assert.Single(errors);
        Assert.Contains("password", errors.Keys);
        Assert.Contains("Password must contain at least one uppercase letter", errors["password"]);
        Assert.Contains("Password must contain at least one digit", errors["password"]);
    }
    
    [Fact]
    public void ValidateLoginRequest_WhenFieldsAreEmpty_ShouldReturnErrors()
    {
        var request = new LoginRequest
        {
            Email = "",
            Password = ""
        };

        var errors = _authValidator.ValidateLoginRequest(request);

        Assert.Contains("Email is required", errors["email"]);
        Assert.Contains("Password is required", errors["password"]);
    }
    
    [Fact]
    public void ValidateLoginRequest_WhenRequestIsValid_ShouldNotReturnErrors()
    {
        var request = new LoginRequest
        {
            Email = "test@test.com",
            Password = "Password1"
        };

        var errors = _authValidator.ValidateLoginRequest(request);

        Assert.False(_authValidator.HasErrors);
        Assert.Empty(errors);
    }
}