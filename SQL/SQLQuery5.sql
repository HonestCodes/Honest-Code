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
/****** Object:  StoredProcedure [dbo].[Meals_User_SelectByUserId]    Script Date: 10/29/2017 6:37:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

	ALTER proc [dbo].[Meals_User_SelectByUserId]
	@UserId int

/*	-- test code --

Declare @UserId int = 2; 

Execute dbo.Meals_User_SelectByUserId 
	@UserId

*/

as

BEGIN

WITH mcte as
(


SELECT		    m.Title
			  , m.Id as MealId
			  , m.Description
			  , u.FirstName
			  , u.LastName
			  , u.Id as UserId
			  , f.FileName
			  , f.FileTypeId, f.Id as FileId
	  
,ROW_NUMBER () OVER(PARTITION BY m.Id Order by mp.PhotoId) AS RN

		FROM dbo.Meals m Join dbo.Users u
				ON	m.UserId = u.Id
				Join dbo.MealPhotos mp
				On	mp.MealId = m.Id
				Join dbo.Files f
				ON	f.id = mp.PhotoId
		Where m.UserId = @UserId
)


SELECT TOP 3 * From mcte 
WHERE RN = 1
Order by NEWID()

END

