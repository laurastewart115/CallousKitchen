USE [CallousHippo]
GO

/****** Object:  StoredProcedure [dbo].[AccountLogin]    Script Date: 4/3/2020 5:03:35 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Peter Szadurski
-- Create date: 
-- Description:	
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[AccountLogin] 
	-- Add the parameters for the stored procedure here
	@email varchar(255),
	@password varchar(255)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
	(Select * from [User] where Lower(Email) = Lower(@email) 
	and [Password] = @password )

		
END
GO

