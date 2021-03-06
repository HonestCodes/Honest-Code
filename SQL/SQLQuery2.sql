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
/****** Object:  StoredProcedure [dbo].[CMSContent_SelectContentByPage]    Script Date: 10/29/2017 6:34:30 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER Proc [dbo].[CMSContent_SelectContentByPage]
		@PageId int

/*
		declare @PageId int = 3
execute dbo.CMSContent_SelectContentByPage
		@PageId 


*/
as

Begin

				SELECT	k.KeyName
				, p.Id as PageID 
				, p.URL
				, k.Type
				, k.Id as KeyId
				, c.*
				FROM	dbo.CMSTemplateKeys k inner join dbo.CMSPages p
						on p.TemplateId = k.TemplateId
						Left OUTER JOIN dbo.CMSContent c  
						on c.CMSPageId = p.Id and k.Id = c.TemplateKeyId
				
				
				WHERE p.Id = @PageId;

END