CREATE FUNCTION Sequences_Employees (
	@nombre Int
)
RETURNS INT AS
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

