
using System.Diagnostics;
using System.Collections.Generic;
using System.ClientModel;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using Microsoft.Extensions.Logging;
using OpenAI;

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4o-mini";
var key= Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY") ?? throw new InvalidOperationException("AZURE_OPENAI_KEY is not set.");
var mcpendpoint= Environment.GetEnvironmentVariable("MCP_ENDPOINT") ?? throw new InvalidOperationException("MCP_ENDPOINT is not set.");

// Create an MCPClient for the GitHub server
await using var mcpClient = await McpClient.CreateAsync(new StdioClientTransport(new()
{
    Name = "MCPServer",
    Command = "npx",
    Arguments = ["-y", "--verbose", "@modelcontextprotocol/server-github"],
}));


// We can customize a shared HttpClient with a custom handler if desired
// var customkey= Environment.GetEnvironmentVariable("REMOTE_MCP_KEY") ?? throw new InvalidOperationException("REMOTEMCPKEY is not set.");

// using var sharedHandler = new SocketsHttpHandler
// {
//     PooledConnectionLifetime = TimeSpan.FromMinutes(2),
//     PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1)
// };
// using var httpClient = new HttpClient(sharedHandler);
// var transport = new HttpClientTransport(new()
// {
//     Endpoint = new Uri(mcpendpoint),
//     Name = "Secure Weather Client",
//     AdditionalHeaders = new Dictionary<string, string> { ["x-functions-key"] = customkey }
// }, httpClient);


// await using var mcpClient = await McpClient.CreateAsync(transport);

// Retrieve the list of tools available on the GitHub server
var mcpTools = await mcpClient.ListToolsAsync().ConfigureAwait(false);
var githubtextinstructions = "You answer questions related to GitHub repositories only.";
//var qrcodeinstructions = "You generate QR codes based on the input text.";
AIAgent agent = new AzureOpenAIClient(
    new Uri(endpoint),
    new ApiKeyCredential(key))
     .GetChatClient(deploymentName)
     .CreateAIAgent(instructions: githubtextinstructions, tools: [.. mcpTools.Cast<AITool>()]);

//Console.WriteLine(await agent.RunAsync("Generate a QR code Ascii art for the URL:http://www.zecloud.fr"));
// Invoke the agent and output the text result.
Console.WriteLine(await agent.RunAsync("Summarize the last four commits to the microsoft/semantic-kernel repository?"));