namespace PasswordManager.Application
{
    public class Result<T> : Result
    {
        public T? Value { get; private init; }

        public static Result<T> Ok(T value) => new Result<T> { Value = value };
    }
}