Create Trigger TR_Employees On dbo.Employees Instead Of Insert As
Begin
  Insert dbo.Employees
  Select Next Value For dbo.SEQ
  From inserted i
  Where i.PKEMPL Is Null;
  Insert dbo.Employees
  Select i.PKEMPL
  From inserted i
  Where i.PKEMPL Is Not Null;
End