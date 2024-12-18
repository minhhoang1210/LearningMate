using FluentResults;
using LearningMate.Domain.Entities;

namespace LearningMate.Domain.RepositoryContracts;

public interface IExamsRepository
{
    Task<Result<bool>> CheckExamExists(Guid examId);
    Task<Result<int>> AddExamAsync(Exam exam);
    Task<Result<Exam>> GetExamOverviewAsync(Guid examId);
    Task<Result<Exam>> GetExamWritingTopicsAsync(Guid examId);
    Task<Result<Exam>> GetExamSpeakingTopicsAsync(Guid examId);
    Task<Result<Exam>> GetExamListeningTopicsAsync(Guid examId);
    Task<Result<Exam>> GetExamReadingTopicsAsync(Guid examId);
    Task<Result<IEnumerable<Exam>>> GetExams();
}
