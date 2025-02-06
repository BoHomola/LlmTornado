using LlmTornado.Assistants;
using LlmTornado.Chat;
using LlmTornado.Chat.Models;
using LlmTornado.Common;
using LlmTornado.Threads;

namespace LlmTornado.Demo;

public static class ThreadsDemo
{
    private static TornadoThread? generatedThread;
    private static Message? generatedMessage;
    private static TornadoRun? generatedTornadoRun;

    [TornadoTest]
    public static async Task<TornadoThread> CreateThread()
    {
        HttpCallResult<TornadoThread> thread = await Program.Connect().Threads.CreateThreadAsync();
        Console.WriteLine(thread.Response);
        generatedThread = thread.Data;
        return thread.Data!;
    }

    [TornadoTest]
    public static async Task<TornadoThread> RetrieveThread()
    {
        generatedThread ??= await CreateThread();
        HttpCallResult<TornadoThread> response = await Program.Connect().Threads.RetrieveThreadAsync(generatedThread!.Id);
        Console.WriteLine(response.Response);
        return response.Data!;
    }

    [TornadoTest]
    public static async Task<TornadoThread> ModifyThread()
    {
        generatedThread ??= await CreateThread();
        HttpCallResult<TornadoThread>? response = await Program.Connect().Threads.ModifyThreadAsync(generatedThread.Id,
            new ModifyThreadRequest
            {
                Metadata = new Dictionary<string, string>
                {
                    {"key1", "value1"},
                    {"key2", "value2"}
                }
            });
        Console.WriteLine(response.Response);
        return response.Data!;
    }

    [TornadoTest]
    public static async Task DeleteThread()
    {
        generatedThread ??= await CreateThread();
        HttpCallResult<bool> deleted = await Program.Connect().Threads.DeleteThreadAsync(generatedThread.Id);
        generatedThread = null;
        generatedMessage = null;
        generatedTornadoRun = null;
        Console.WriteLine(deleted.Response);
    }

    [TornadoTest]
    public static async Task<Message> CreateMessage()
    {
        generatedThread ??= await CreateThread();
        Message response = await CreateMessage(generatedThread.Id,
            "I need to think of a magic spell that turns cows into sheep.");
        generatedMessage = response;
        return response;
    }

    public static async Task<Message> CreateMessage(string threadId, string content)
    {
        HttpCallResult<Message> response = await Program.Connect().Threads.CreateMessageAsync(threadId,
            new CreateMessageRequest(content));
        Console.WriteLine(response.Response);
        return response.Data!;
    }

    [TornadoTest]
    public static async Task<Message> RetrieveMessage()
    {
        generatedThread ??= await CreateThread();
        generatedMessage ??= await CreateMessage();

        HttpCallResult<Message> response =
            await Program.Connect().Threads.RetrieveMessageAsync(generatedThread.Id, generatedMessage.Id);
        Console.WriteLine(response.Response);
        return response.Data!;
    }

    [TornadoTest]
    public static async Task<IReadOnlyList<Message>> ListMessages()
    {
        generatedThread ??= await CreateThread();
        generatedMessage ??= await CreateMessage();

        HttpCallResult<ListResponse<Message>> response =
            await Program.Connect().Threads.ListMessagesAsync(generatedThread.Id);
        Console.WriteLine(response.Response);
        return response.Data!.Items;
    }

    [TornadoTest]
    public static async Task<Message> ModifyMessage()
    {
        generatedThread ??= await CreateThread();
        generatedMessage ??= await CreateMessage();

        HttpCallResult<Message> response = await Program.Connect().Threads.ModifyMessageAsync(generatedThread.Id,
            generatedMessage.Id, new ModifyMessageRequest
            {
                Metadata = new Dictionary<string, string>
                {
                    {"key1", "value1"},
                    {"key2", "value2"}
                }
            });

        Console.WriteLine(response.Response);
        return response.Data!;
    }

    [TornadoTest]
    public static async Task<bool> DeleteMessage()
    {
        generatedThread = await CreateThread();
        generatedMessage = await CreateMessage();

        HttpCallResult<bool> response =
            await Program.Connect().Threads.DeleteMessageAsync(generatedThread.Id, generatedMessage.Id);
        generatedMessage = null;
        Console.WriteLine(response.Response);
        return response.Data;
    }
    
    public static async Task<TornadoRun> CreateRun(Assistant? assistant = null, string? assistantInstruction = null)
    {
        generatedMessage ??= await CreateMessage();
        assistant ??= await AssistantsDemo.Create();

        HttpCallResult<TornadoRun> response = await Program.Connect().Threads.CreateRunAsync(generatedThread!.Id,
            new CreateRunRequest(assistant!.Id)
            {
                Model = ChatModel.OpenAi.Gpt4.O241120,
                Instructions = assistantInstruction ?? "You are a helpful assistant with the ability to create names of magic spells."
            });
        Console.WriteLine(response.Response);
        generatedTornadoRun = response.Data!;

        return response.Data!;
    }

    [TornadoTest]
    public static async Task<TornadoRun> RetrieveRun()
    {
        generatedTornadoRun ??= await CreateRun();

        HttpCallResult<TornadoRun> response =
            await Program.Connect().Threads.RetrieveRunAsync(generatedThread!.Id, generatedTornadoRun!.Id);
        Console.WriteLine(response.Response);
        return response.Data!;
    }

    [TornadoTest]
    public static async Task<TornadoRun> RetrieveRunAndPollForCompletion()
    {
        generatedTornadoRun ??= await CreateRun();

        while (true)
        {
            HttpCallResult<TornadoRun> response = await Program.Connect().Threads
                .RetrieveRunAsync(generatedThread!.Id, generatedTornadoRun!.Id);
            if (response.Data!.Status == RunStatus.Completed)
            {
                IReadOnlyList<Message> messages = await ListMessages();
                Console.WriteLine(response.Response);

                foreach (Message message in messages.Reverse())
                {
                    foreach (MessageContent content in message.Content)
                    {
                        if (content is MessageContentTextResponse text)
                        {
                            Console.WriteLine($"{message.Role}: {text.MessageContentTextData?.Value}");
                        }
                    }
                }

                return response.Data!;
            }

            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }

    public static async Task<TornadoRun> ModifyRun()
    {
        generatedTornadoRun ??= await CreateRun();

        HttpCallResult<TornadoRun> response = await Program.Connect().Threads.ModifyRunAsync(generatedThread!.Id,
            generatedTornadoRun!.Id,
            new ModifyRunRequest
            {
                Metadata = new Dictionary<string, string>
                {
                    {"key1", "value1"},
                    {"key2", "value2"}
                }
            });
        Console.WriteLine(response.Response);
        generatedTornadoRun = response.Data!;
        return response.Data!;
    }

    [TornadoTest]
    public static async Task<IReadOnlyList<TornadoRunStep>> ListRunSteps()
    {
        generatedTornadoRun ??= await RetrieveRunAndPollForCompletion();
        HttpCallResult<ListResponse<TornadoRunStep>> response =
            await Program.Connect().Threads.ListRunStepsAsync(generatedThread!.Id, generatedTornadoRun!.Id);
        Console.WriteLine(response.Response);
        return response.Data!.Items;
    }

    [TornadoTest]
    public static async Task<TornadoRunStep> RetrieveRunStep()
    {
        IReadOnlyList<TornadoRunStep> runSteps = await ListRunSteps();
        HttpCallResult<TornadoRunStep> response = await Program.Connect().Threads
            .RetrieveRunStepAsync(generatedThread!.Id, generatedTornadoRun!.Id, runSteps.FirstOrDefault()!.Id);
        Console.WriteLine(response.Response);
        return response.Data!;
    }

    [TornadoTest]
    public static async Task<TornadoRun> ExtractInfoFromFile()
    {
        Assistant assistant = await AssistantsDemo.CreateFileSearchAssistant();
        generatedThread = await CreateThread();
        generatedMessage =
            await CreateMessage(generatedThread.Id, "Please summarize the file in 3 sentences from the file.");
        generatedTornadoRun = await CreateRun(assistant, "You are assistant that analyzes files in a brief way");
        TornadoRun completedRun = await RetrieveRunAndPollForCompletion();
        return completedRun;
    }
}