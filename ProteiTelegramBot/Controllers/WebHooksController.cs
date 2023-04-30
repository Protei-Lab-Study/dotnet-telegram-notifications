using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProteiTelegramBot.Localization;
using ProteiTelegramBot.Models;
using ProteiTelegramBot.Services;
using System.Net;

namespace ProteiTelegramBot.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class WebHooksController : ControllerBase
{
    private readonly ILogger<WebHooksController> _logger;
    private readonly INotifier _notifier;

    public WebHooksController(ILogger<WebHooksController> logger,
        INotifier notifier)
    {
        _logger = logger;
        _notifier = notifier;
    }

    [HttpPost("gitlab")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> GitLabWebHookAsync()
    {
        try
        {
            //считыаем JSON
            var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            //преобразуем в базовый класс для событий
            Event requestEvent = JsonConvert.DeserializeObject<Event>(body);
            _logger.LogInformation(
                 $"Receive new WebHook with kind: {requestEvent.ObjectKind}");
            //в зависимости от типа запроса, вызываем один из методов
            switch (requestEvent.ObjectKind)
            {
                case "pipeline":
                    { 
                        var pipelineEvent = JsonConvert.DeserializeObject<PipelineEvent>(body);
                        if (await GitLabWebHookPipelineAsync(pipelineEvent))
                            return Ok();
                        break;
                    }
                case "merge_request":
                    {
                        
                        var mergeRequestEvent = JsonConvert.DeserializeObject<MergeRequestEvent>(body);
                        if (await GitLabWebHookMergeRequestAsync(mergeRequestEvent))
                            return Ok();
                        break;
                    }
            }

            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            return Problem(statusCode: (int)HttpStatusCode.InternalServerError, detail: e.Message,
                title: Errors.Common_Internal_Server_Error);
        }
    }

    private async Task<bool> GitLabWebHookMergeRequestAsync(MergeRequestEvent mergeRequestEvent)
    {
        try
        {
            _logger.LogInformation(
                        $"Received WebHook is for {mergeRequestEvent.Project.Name}\n\rTitle:{mergeRequestEvent.ObjectAttributes.Title}");
            switch (mergeRequestEvent.ObjectAttributes.Action)
            {
                case "open":
                    {
                        await _notifier.Notify(new MergeRequestOpenedNotification(mergeRequestEvent.ObjectAttributes.Url,
                            mergeRequestEvent.ObjectAttributes.Title,
                            mergeRequestEvent.User.Username));
                        break;
                    }
                case "merge":
                    {
                        await _notifier.Notify(new MergeRequestMergedNotification(mergeRequestEvent.ObjectAttributes.Url,
                            mergeRequestEvent.ObjectAttributes.Title));
                        break;
                    }
            }
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            return false;
        }
    }

    private async Task<bool> GitLabWebHookPipelineAsync(PipelineEvent pipelineEvent)
    {
        try
        {
            _logger.LogInformation(
                           $"Received WebHook Pipeline for {pipelineEvent.Project.Name} " +
                           $"\n\rwith status: {pipelineEvent.ObjectAttributes.Status}");
            if (pipelineEvent.ObjectAttributes.Status != "success")
            {
               
                await _notifier.Notify(new PipelineNotification(pipelineEvent.Project.Url, pipelineEvent.Project.Name, pipelineEvent.ObjectAttributes.Status));
                return true;
            }
            else return false;
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            return false;
        }
    }
}