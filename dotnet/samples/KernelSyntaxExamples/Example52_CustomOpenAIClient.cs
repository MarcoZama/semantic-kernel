﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Azure;
using Azure.AI.OpenAI;
using Azure.Core.Pipeline;
using Microsoft.SemanticKernel;
using RepoUtils;

public static class Example52_CustomOpenAIClient
{
    public static async Task RunAsync()
    {
        Console.WriteLine("======== Using a custom OpenAI client ========");

        string endpoint = TestConfiguration.AzureOpenAI.Endpoint;
        string deploymentName = TestConfiguration.AzureOpenAI.ChatDeploymentName;
        string apiKey = TestConfiguration.AzureOpenAI.ApiKey;

        if (endpoint is null || deploymentName is null || apiKey is null)
        {
            Console.WriteLine("Azure OpenAI credentials not found. Skipping example.");
            return;
        }

        // Create an HttpClient and include your custom header(s)
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("x-my-custom-header", "My custom value");

        // Configure OpenAIClient to use the customized HttpClient
        var clientOptions = new OpenAIClientOptions
        {
            Transport = new HttpClientTransport(httpClient),
        };
        var openAIClient = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey), clientOptions);

        IKernelBuilder builder = Kernel.CreateBuilder();
        builder.AddAzureOpenAIChatCompletion(deploymentName, openAIClient);
        Kernel kernel = builder.Build();

        // Load semantic plugin defined with prompt templates
        string folder = RepoFiles.SamplePluginsPath();

        kernel.ImportPluginFromPromptDirectory(Path.Combine(folder, "FunPlugin"));

        // Run
        var result = await kernel.InvokeAsync(
            kernel.Plugins["FunPlugin"]["Excuses"],
            new() { ["input"] = "I have no homework" }
        );
        Console.WriteLine(result.GetValue<string>());

        httpClient.Dispose();
    }
}
