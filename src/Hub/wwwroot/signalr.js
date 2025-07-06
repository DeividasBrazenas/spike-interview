let connection;

async function setupConnection({ onAudio, onMessage, onError, onSummary }) {
    connection = new signalR.HubConnectionBuilder()
        .withUrl("/hub/calls")
        .withHubProtocol(new signalR.protocols.msgpack.MessagePackHubProtocol())
        .configureLogging(signalR.LogLevel.Information)
        .build();

    connection.on("ReceiveAudio", onAudio);
    connection.on("ReceiveMessage", onMessage);
    connection.on("Error", onError);
    connection.on("ReceiveCallSummary", onSummary);

    await connection.start();
}

function invoke(method, ...args) {
    return connection.invoke(method, ...args);
}

function stopConnection() {
    return connection.stop();
}
