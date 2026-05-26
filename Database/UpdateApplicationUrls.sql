-- ============================================================================
-- UpdateApplicationUrls.sql
-- Point HR / Finance / Inventory Applications at their per-app localhost URLs.
-- Safe to re-run: every UPDATE is keyed on Name and writes the same value.
-- Run against the SSO database (e.g. CoreAppwithSSO_Sso).
-- ============================================================================

USE CoreAppwithSSO_Sso;
GO

UPDATE Applications
SET    BaseUrl = 'http://localhost:5301/hr'
WHERE  Name = 'HR Portal';

UPDATE Applications
SET    BaseUrl = 'http://localhost:5302/finance'
WHERE  Name = 'Finance';

UPDATE Applications
SET    BaseUrl = 'http://localhost:5303/inventory'
WHERE  Name = 'Inventory';
GO

SELECT Id, Name, BaseUrl
FROM   Applications
WHERE  Name IN ('HR Portal', 'Finance', 'Inventory')
ORDER  BY DisplayOrder;
GO
