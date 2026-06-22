using System.Net.Mail;
using System.Text.RegularExpressions;
using Api.Entities;

namespace Api.Features.Auth;

public class AuthValidator
{
    private readonly Dictionary<string, string[]> _errors = new();

    public bool HasErrors => _errors.Count > 0;
    public Dictionary<string, string[]> Errors => _errors;

    // allows username to have letter uppercase/lowercase, numbers 0-9 & underscores
    private static readonly Regex UsernameRegex = new(
        @"^[a-zA-Z0-9_]+$",
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

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public AuthValidator ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            AddError("email", "Email is required");
            return this;
        }

        if (!IsValidEmail(email))
            AddError("email", "Email is not a valid email address");

        return this;
    }

    public AuthValidator ValidateUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            AddError("username", "Username is required");
            return this;
        }

        if (username.Length < 3)
            AddError("username", "Username must be at least 3 characters");

        if (username.Length > 100)
            AddError("username", "Username must be at most 100 characters");

        if (!UsernameRegex.IsMatch(username))
            AddError("username", "Username can only contain letters, digits and underscores");

        return this;
    }

    public AuthValidator ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            AddError("password", "Password is required");
            return this;
        }

        if (password.Length < 8)
            AddError("password", "Password must be at least 8 characters");

        if (password.Length > 64)
            AddError("password", "Password must be at most 64 characters");

        if (!password.Any(char.IsUpper))
            AddError("password", "Password must contain at least one uppercase letter");

        if (!password.Any(char.IsLower))
            AddError("password", "Password must contain at least one lowercase letter");

        if (!password.Any(char.IsDigit))
            AddError("password", "Password must contain at least one digit");

        return this;
    }

    public Dictionary<string, string[]> ValidateRegisterRequest(RegisterRequest request)
    {
        ValidateEmail(request.Email);
        ValidateUsername(request.Username);
        ValidatePassword(request.Password);
        return Errors;
    }

    public Dictionary<string, string[]> ValidateLoginRequest(LoginRequest request)
    {
        ValidateEmail(request.Email);
        ValidatePassword(request.Password);
        return Errors;
    }
}