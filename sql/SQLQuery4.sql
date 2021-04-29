   CREATE PROCEDURE Sequences_Employees
      @nombre INT,
      @ID INT OUTPUT
   AS
   BEGIN
	DECLARE @cnt INT = 0;
	DECLARE @result TABLE(ID INT);

	WHILE @cnt < @nombre

		BEGIN
			Insert @result select (NEXT VALUE FOR seq)
		   SET @cnt = @cnt + 1;
		END;

 
    RETURN (select ID FROM @result)
END;

DECLARE @result TABLE(ID INT)
insert @result
exec dbo.Sequences_Employees @nombre = 2

SELECT * FROM @result