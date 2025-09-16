using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using FEwS.Forums.Domain.Models;
using FEwS.Forums.Domain.UseCases.GetTopics;

namespace FEwS.Forums.Storage.Storages;

internal class GetTopicsStorage(
    ForumDbContext dbContext,
    IMapper mapper) : IGetTopicsStorage
{
    public async Task<TopicsPagedResult> GetTopicsAsync(
        Guid forumId, int skip, int take, CancellationToken cancellationToken)
    {
        IQueryable<Entities.Topic> query = dbContext.Topics.Where(t => t.ForumId == forumId);

        int totalCount = await query.CountAsync(cancellationToken);

        TopicReadModel[] resources = await dbContext.Database.SqlQuery<Models.TopicReadModel>($@"
            SELECT
                t.""TopicId"" as ""TopicId"",
                t.""ForumId"" as ""ForumId"",
                t.""UserId"" as ""UserId"",
                t.""Title"" as ""Title"",
                t.""CreatedAt"" as ""CreatedAt"",
                COALESCE(c.TotalCommentsCount, 0) as ""TotalCommentsCount"",
                c.""CreatedAt"" as ""LastCommentCreatedAt"",
                c.""CommentId"" as ""LastCommentId""
            FROM ""Topics"" as t
            LEFT JOIN (
                SELECT
                    ""TopicId"",
                    ""CommentId"",
                    ""CreatedAt"",
                    COUNT(*) OVER (PARTITION BY ""TopicId"") as TotalCommentsCount,
                    ROW_NUMBER() OVER (PARTITION BY ""TopicId"" ORDER BY ""CreatedAt"" DESC) rn
                FROM ""Comments""
            ) as c ON t.""TopicId"" = c.""TopicId"" AND c.rn = 1
            WHERE t.""ForumId"" = {forumId}
            ORDER BY
                COALESCE(c.""CreatedAt"", t.""CreatedAt"") DESC
            LIMIT {take} OFFSET {skip}")
            .ProjectTo<TopicReadModel>(mapper.ConfigurationProvider)
            .ToArrayAsync(cancellationToken);

        return new TopicsPagedResult(resources, totalCount);
    }
}