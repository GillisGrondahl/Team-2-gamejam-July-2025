using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface ILeaderboardService
{
    Task<IReadOnlyList<LeaderboardEntry>> GetTopEntriesAsync(int limit, CancellationToken cancellationToken = default);
    Task SubmitScoreAsync(
    string playerName,
    int score,
    CancellationToken cancellationToken = default);

}
