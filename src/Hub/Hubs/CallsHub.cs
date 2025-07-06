using Microsoft.AspNetCore.SignalR;
using Spike.Application.Exceptions;
using Spike.Application.Services.Abstractions;
using Spike.Hub.Contracts.Requests;
using Spike.Hub.Mappers;
using Spike.Hub.Validators;

namespace Spike.Hub.Hubs;

public class CallsHub : Microsoft.AspNetCore.SignalR.Hub
{
    private ICallService _callService;
    private IDtoValidator _validator;
    private IMapper _mapper;

    public CallsHub(ICallService callService, IDtoValidator validator, IMapper mapper)
    {
        _callService = callService;
        _validator = validator;
        _mapper = mapper;
    }

    public async Task<string> StartCall(StartCallRequest request)
    {
        var patientDto = request.Patient;
        
        _validator.Validate(patientDto);

        var patient = _mapper.MapPatient(request.Patient);

        var call = await _callService.StartCall(patient, request.VoiceId, Context.ConnectionAborted);

        return call.Id.ToString();
    }

    public async Task EndCall(EndCallRequest request)
    {
        if (!Guid.TryParse(request.CallId, out var parsedId))
        {
            throw new ValidationException("CallId is not a valid GUID.");
        }

        var call = await _callService.EndCall(parsedId, Context.ConnectionAborted);

        var summary = _mapper.MapCallSummary(call.Summary);

        await Clients.Caller.SendAsync("ReceiveCallSummary", summary);
    }

    public async Task Interact(string callId, byte[] audioBytes)
    {
        if (!Guid.TryParse(callId, out var parsedId))
        {
            throw new ValidationException("CallId is not a valid GUID.");
        }

        var result = await _callService.Interact(parsedId, audioBytes, Context.ConnectionAborted);

        await Clients.Caller.SendAsync("ReceiveAudio", result);
    }
}