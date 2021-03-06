/*    ==Scripting Parameters==

    Source Server Version : SQL Server 2016 (13.0.4001)
    Source Database Engine Edition : Microsoft SQL Server Enterprise Edition
    Source Database Engine Type : Standalone SQL Server

    Target Server Version : SQL Server 2017
    Target Database Engine Edition : Microsoft SQL Server Standard Edition
    Target Database Engine Type : Standalone SQL Server
*/

USE [C40_GSwap]
GO
/****** Object:  StoredProcedure [dbo].[CMSPages_GetByURL]    Script Date: 10/29/2017 6:32:47 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER proc [dbo].[CMSPages_GetByURL]
@URL nvarchar(150)

/* ---- TEST CODE -----

	declare	@URL nvarchar(150) = 'featuredMeal'
	execute dbo.CMSPages_GetByURL
			@URL


*/
as

BEGIN

		SELECT		cp.TemplateId
					, ct.Path as HtmlPath
					, cp.Id as PageId
	FROM dbo.CMSPages cp Join dbo.CMSTemplates ct
				on cp.TemplateId = ct.Id
				
				Where cp.URL = @URL;

		SELECT
				
				 c.Id 
				, ct.KeyName
				, ct.Id as TemplateKeyId
				, ct.Type as KeyType
				, c.Value
				

	FROM dbo.CMSPages cp 
				JOIN dbo.CMSContent c 
				on c.CMSPageId = cp.Id 
				Join dbo.CMSTemplateKeys ct
				on ct.Id = c.TemplateKeyId and ct.TemplateId = cp.TemplateId

				Where cp.URL = @URL;





END