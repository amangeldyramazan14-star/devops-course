namespace DemoApi.Errors;

// Бизнес-ошибки: обработчик превращает их в 404 / 409
public sealed class NotFoundException(string message) : Exception(message);

public sealed class ConflictException(string message) : Exception(message);
