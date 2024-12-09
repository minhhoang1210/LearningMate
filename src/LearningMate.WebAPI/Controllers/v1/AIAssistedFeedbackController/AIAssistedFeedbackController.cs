using LearningMate.AI.ServiceContracts.EnglishSkillsAIAssistedFeedbackServiceContract;
using LearningMate.Core.ConfigurationOptions.AppServer;
using LearningMate.Core.ServiceContracts.ListeningTopicsServiceContract;
using LearningMate.Core.ServiceContracts.ReadingTopicsServiceContract;
using LearningMate.Domain.IdentityEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace LearningMate.WebAPI.Controllers.v1.AIAssistedFeedbackController;

public partial class AIAssistedFeedbackController(
    UserManager<AppUser> userManager,
    IOptions<MyAppServerConfiguration> myAppServerConfiguration,
    ILogger<AIAssistedFeedbackController> logger,
    IReadingTopicsService readingTopicsService,
    IListeningTopicsService listeningTopicsService,
    IEnglishSkillsAIAssistedFeedbackService feedbackService
) : CustomControllerBaseV1(userManager, myAppServerConfiguration)
{
    private readonly ILogger<AIAssistedFeedbackController> _logger = logger;
    private readonly IReadingTopicsService _readingTopicsService = readingTopicsService;
    private readonly IListeningTopicsService _listeningTopicsService = listeningTopicsService;
    private readonly IEnglishSkillsAIAssistedFeedbackService _feedbackService = feedbackService;
}
