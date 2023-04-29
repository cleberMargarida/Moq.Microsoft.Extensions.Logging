using Microsoft.Extensions.Logging;
using Moq;

var mock = new Mock<ILogger>();

mock.Setup(x => x.Log(
    LogLevel.Information,
    It.IsAny<EventId>(),
    It.IsAny<It.IsSubtype<IReadOnlyList<KeyValuePair<string, object?>>>>(),
    It.IsAny<Exception>(),
    It.IsAny<Func<It.IsSubtype<IReadOnlyList<KeyValuePair<string, object?>>>, Exception?, string>>()))
    .Callback
        <LogLevel, 
        EventId,
        object, 
        Exception,
        Delegate> 
     ((logLevel, eventId, state , exception, formatter) => 
     {
     
     })
    .Verifiable();

var log = mock.Object;
var date = DateTime.Now;
log.LogInformation("message {date}", date);

mock.Verify(x => x.Log(
    LogLevel.Information,
    It.IsAny<EventId>(),
    It.IsAny<It.IsSubtype<IReadOnlyList<KeyValuePair<string, object?>>>>(),
    It.IsAny<Exception>(),
    It.IsAny<Func<It.IsSubtype<IReadOnlyList<KeyValuePair<string, object?>>>, Exception?, string>>()));
