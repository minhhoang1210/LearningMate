using System.Text.Json;
using Dapper;
using FluentResults;
using LearningMate.Core.ErrorMessages;
using LearningMate.Core.Errors;
using LearningMate.Core.LoggingMessages;
using LearningMate.Domain.Entities.Listening;
using LearningMate.Domain.Entities.QuestionTypes.MultipleChoice;
using LearningMate.Domain.RepositoryContracts;
using LearningMate.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace LearningMate.Infrastructure.Repositories;

public class ListeningTopicsRepository(
    ILogger<ListeningTopicsRepository> logger,
    IDbConnectionFactory dbConnectionFactory
) : IListeningTopicsRepository
{
    private readonly ILogger<ListeningTopicsRepository> _logger = logger;
    private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

    public async Task<Result<int>> AddTopicAsync(ListeningTopic topic)
    {
        using var dbConnection = await _dbConnectionFactory.CreateConnectionAsync();
        var sqlCommand = """
                INSERT INTO listening_topics
                    (
                        id,
                        category,
                        title,
                        content,
                        score_band,
                        score,
                        exam_id
                    )
                VALUES
                    (
                        @Id,
                        @Category,
                        @Title,
                        @Content,
                        @ScoreBand,
                        @Score,
                        @ExamId
                    );
            """;

        var totalAffectedRows = await dbConnection.ExecuteAsync(sqlCommand, topic);

        if (totalAffectedRows == 0)
        {
            _logger.LogWarning(CommonLoggingMessages.FailedToCreate, "new listening topic");
            return new ProblemDetailsError(CommonErrorMessages.FailedTo("add new listening topic"));
        }

        return totalAffectedRows;
    }

    public async Task<Result<bool>> CheckListeningTopicExistsAsync(Guid topicId)
    {
        using var dbConnection = await _dbConnectionFactory.CreateConnectionAsync();
        var sqlQuery = "SELECT COUNT(DISTINCT 1) from listening_topics WHERE id=@topicId";
        var isExist = dbConnection.ExecuteScalar<bool>(sqlQuery, new { topicId });

        if (isExist == false)
        {
            _logger.LogWarning(
                CommonLoggingMessages.RecordNotFoundWithId,
                nameof(ListeningTopic),
                topicId
            );
            return new ProblemDetailsError(
                CommonErrorMessages.RecordNotFoundWithId(nameof(ListeningTopic), topicId)
            );
        }

        return isExist;
    }

    public async Task<Result<ListeningTopic>> GetListeningTopicWithSolutionById(Guid id)
    {
        using var dbConnection = await _dbConnectionFactory.CreateConnectionAsync();
        var sqlQuery = """
                SELECT
                    lt.id, lt.content,
                    ltq.id, ltq.content, ltq.serialized_answer_options
                FROM listening_topics lt
                LEFT JOIN listening_topic_questions ltq ON lt.id = ltq.topic_id
                WHERE lt.id = @id;
            """;
        ListeningTopic? listeningTopic = null;
        var queryResult = await dbConnection.QueryAsync<
            ListeningTopic,
            ListeningTopicQuestion,
            ListeningTopic
        >(
            sqlQuery,
            (topic, topicQuestion) =>
            {
                listeningTopic ??= topic;
                if (topicQuestion is not null)
                {
                    var answerOptions = JsonSerializer.Deserialize<
                        List<MultipleChoiceAnswerOption>
                    >(topicQuestion.SerializedAnswerOptions ?? "[]");
                    if (answerOptions is not null)
                    {
                        topicQuestion.AnswerOptions = answerOptions;
                    }
                    listeningTopic.Questions ??= [];
                    listeningTopic.Questions.Add(topicQuestion);
                }
                return listeningTopic;
            },
            new { id },
            splitOn: "id"
        );
        var listeningTopicResult = queryResult.FirstOrDefault();
        if (listeningTopicResult is null)
        {
            _logger.LogError(
                CommonLoggingMessages.FailedToPerformActionWithId,
                "get listening topic with questions",
                id
            );
            return new ProblemDetailsError(
                CommonErrorMessages.FailedTo("get listening topic with questions")
            );
        }
        return listeningTopicResult;
    }

    public async Task<Result<ListeningTopic>> GetTopicById(Guid id)
    {
        using var dbConnection = await _dbConnectionFactory.CreateConnectionAsync();
        var sqlQuery = """
                SELECT id, category, title, content, score_band, score
                FROM listening_topics
                WHERE id = @id;
            """;
        var queryResult = await dbConnection.QueryFirstAsync<ListeningTopic>(sqlQuery, new { id });
        if (queryResult is null)
        {
            _logger.LogError(
                CommonLoggingMessages.FailedToPerformActionWithId,
                "get listening topic content",
                id
            );
            return new ProblemDetailsError(
                CommonErrorMessages.FailedTo("get listening topic content")
            );
        }
        return queryResult;
    }
}
