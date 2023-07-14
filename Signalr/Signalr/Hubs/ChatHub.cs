using Microsoft.AspNetCore.SignalR;
using Signalr.Models;

namespace Signalr.Hubs;

public class ChatHub : Hub
{
	private readonly string _botUser;
	private readonly IDictionary<string, UserConnection> _connections;

	public ChatHub(IDictionary<string, UserConnection> connections)
	{
		_botUser = "MyChat Bot";
		_connections = connections;
	}

	public override Task OnConnectedAsync()
	{
		return base.OnConnectedAsync();
	}

	public override Task OnDisconnectedAsync(Exception? exception)
	{
		if (_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
		{
			_connections.Remove(Context.ConnectionId);
			Clients.Group(userConnection.Room).SendAsync("ReceiveMessage", _botUser, $"{userConnection.User} has left");
			SendConnectedUsers(userConnection.Room);
		}

		return base.OnDisconnectedAsync(exception);
	}

	public async Task SendMessage(string message)
	{
		if (_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
		{
			if (string.IsNullOrEmpty(userConnection.Room)) return;
			await Clients.Group(userConnection.Room).SendAsync("ReceiveMessage", userConnection.User, message);
		}
	}

	public async Task JoinRoom(UserConnection userConnection)
	{
		string Room = userConnection.Room != null ? userConnection.Room : "";
		if (string.IsNullOrEmpty(Room)) return;

		_connections[Context.ConnectionId] = userConnection;

		await Groups.AddToGroupAsync(Context.ConnectionId, Room);
		await Clients.Group(Room).SendAsync("ReceiveMessage", _botUser,
			$"{userConnection.User} has joined {userConnection.Room}");
		await SendConnectedUsers(Room);
	}

	public Task SendConnectedUsers(string room)
	{
		var users = _connections.Values.Where(c => c.Room == room).Select(c => c.User);
		return Clients.Group(room).SendAsync("UsersInRoom", users);
	}
}