-- =============================================================================
-- Election Tracker — [ELT].[Constituency] stored procedures
--   * [ELT].[GetConstituencyList]  — list constituencies, optionally filtered by StateId.
--   * [ELT].[ConstituencySearch]   — paged/sorted dynamic search (see
--                                    ConstituencyRepository + CommonUtl.SetDynamicParametersData).
-- Target: each TENANT database. Engine: SQL Server.
-- =============================================================================

-- GetConstituencyList -------------------------------------------------------
CREATE OR ALTER PROCEDURE [ELT].[GetConstituencyList]
    @StateId INT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SELECT *
    FROM [ELT].[Constituency]
    WHERE (@StateId = 0 OR [StateId] = @StateId)
    ORDER BY [ConstituencyId];
END
GO

-- ConstituencySearch --------------------------------------------------------
-- @DynamicSearchParameters is the XML string emitted by SetDynamicParametersData.
-- @PageNo is 1-based; 0 or 1 both return the first page.
CREATE OR ALTER PROCEDURE [ELT].[ConstituencySearch]
    @SortColumn NVARCHAR(128) = NULL,
    @SortOrder  NVARCHAR(4)   = NULL,
    @PageNo     INT           = 0,
    @PageSize   INT           = 10,
    @DynamicSearchParameters NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF (@PageSize IS NULL OR @PageSize <= 0) SET @PageSize = 10;
    DECLARE @offset INT = CASE WHEN @PageNo > 1 THEN (@PageNo - 1) * @PageSize ELSE 0 END;
    DECLARE @where  NVARCHAR(MAX) = N'';
    DECLARE @sql    NVARCHAR(MAX);

    IF (@DynamicSearchParameters IS NOT NULL AND LTRIM(RTRIM(@DynamicSearchParameters)) <> N'')
    BEGIN
        DECLARE @xml XML = CAST(@DynamicSearchParameters AS XML);

        SELECT @where = @where +
            CASE WHEN @where = N'' THEN N' WHERE ' ELSE N' AND ' END +
            QUOTENAME(s.col) +
            CASE s.op
                WHEN '='  THEN N' = N'''  + s.val + N''''
                WHEN '!=' THEN N' <> N''' + s.val + N''''
                WHEN '<>' THEN N' <> N''' + s.val + N''''
                WHEN '>'  THEN N' > N'''  + s.val + N''''
                WHEN '<'  THEN N' < N'''  + s.val + N''''
                WHEN '>=' THEN N' >= N''' + s.val + N''''
                WHEN '<=' THEN N' <= N''' + s.val + N''''
                ELSE N' LIKE N''%' + s.val + N'%'''
            END
        FROM (
            SELECT
                node.value('@DynamicColumnName', 'NVARCHAR(128)') AS col,
                REPLACE(ISNULL(node.value('@DynamicColumnValue', 'NVARCHAR(MAX)'), N''), '''', '''''') AS val,
                LOWER(LTRIM(RTRIM(ISNULL(node.value('@Operator', 'NVARCHAR(20)'), N'')))) AS op
            FROM @xml.nodes('/xDynamicSearchParameters/_xDynamicSearchParameter') AS T(node)
        ) s
        WHERE s.col IS NOT NULL AND s.col <> N'';
    END

    IF (@SortColumn IS NULL OR LTRIM(RTRIM(@SortColumn)) = N'') SET @SortColumn = N'ConstituencyId';
    IF (UPPER(ISNULL(@SortOrder, N'ASC')) NOT IN (N'ASC', N'DESC')) SET @SortOrder = N'ASC';

    SET @sql =
        N'SELECT *, COUNT(*) OVER() AS [TotalRecord] FROM [ELT].[Constituency]' + @where +
        N' ORDER BY ' + QUOTENAME(@SortColumn) + N' ' + @SortOrder +
        N' OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;';

    EXEC sp_executesql @sql, N'@Offset INT, @PageSize INT', @Offset = @offset, @PageSize = @PageSize;
END
GO
