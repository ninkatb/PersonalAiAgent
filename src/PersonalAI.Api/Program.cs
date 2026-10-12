// No HTTP listener is started until STORY-025 supplies the authenticated
// private handshake. Do not use ASPNETCORE_URLS or an unauthenticated endpoint.
Console.Error.WriteLine("API_TRANSPORT_NOT_CONFIGURED");
return 78;
