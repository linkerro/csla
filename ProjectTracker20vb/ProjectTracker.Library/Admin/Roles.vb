Imports System.Data.SqlClient

Namespace Admin

  ''' <summary>
  ''' Used to maintain the list of roles
  ''' in the system.
  ''' </summary>
  <Serializable()> _
  Public Class Roles
    Inherits BusinessListBase(Of Roles, Role)

#Region " Business Methods "

    ''' <summary>
    ''' Remove a role based on the role's
    ''' id value.
    ''' </summary>
    ''' <param name="id">Id value of the role to remove.</param>
    Public Overloads Sub Remove(ByVal id As Integer)

      For Each item As Role In Me
        If item.Id = id Then
          Remove(item)
          Exit For
        End If
      Next

    End Sub

    ''' <summary>
    ''' Get a role bsaed on its id value.
    ''' </summary>
    ''' <param name="id">Id value of the role to return.</param>
    Public Function GetRoleById(ByVal id As Integer) As Role

      For Each item As Role In Me
        If item.Id = id Then
          Return item
        End If
      Next
      Return Nothing

    End Function

    Protected Overrides Function AddNewCore() As Object

      Dim item As Role = Role.NewRole
      Add(item)
      Return item

    End Function

#End Region

#Region " Authorization Rules "

    Public Shared Function CanAddObject() As Boolean

      Return My.User.IsInRole("Administrator")

    End Function

    Public Shared Function CanGetObject() As Boolean

      Return True

    End Function

    Public Shared Function CanDeleteObject() As Boolean

      Dim result As Boolean
      If My.User.IsInRole("Administrator") Then
        result = True
      End If
      Return result

    End Function

    Public Shared Function CanSaveObject() As Boolean

      Return My.User.IsInRole("Administrator")

    End Function

#End Region

#Region " Constructors "

    Private Sub New()

      Me.AllowNew = True

    End Sub

#End Region

#Region " Criteria "

    <Serializable()> _
    Private Class Criteria
      ' no criteria
    End Class

#End Region

#Region " Factory Methods "

    Public Shared Function GetRoles() As Roles

      Return DataPortal.Fetch(Of Roles)(New Criteria)

    End Function

#End Region

#Region " Data Access "

    Public Overrides Function Save() As Roles

      ' see if save is allowed
      If Not CanSaveObject() Then
        Throw New System.Security.SecurityException("User not authorized to save roles")
      End If

      ' do the save
      Dim result As Roles
      result = MyBase.Save()

      ' this runs on the client and invalidates
      ' the RoleList cache
      RoleList.InvalidateCache()
      Return result

    End Function

    Protected Overrides Sub DataPortal_OnDataPortalInvokeComplete( _
      ByVal e As Csla.DataPortalEventArgs)

      If ApplicationContext.ExecutionLocation = ApplicationContext.ExecutionLocations.Server Then
        ' this runs on the server and invalidates
        ' the RoleList cache
        RoleList.InvalidateCache()
      End If

    End Sub

    Private Overloads Sub DataPortal_Fetch(ByVal criteria As Criteria)

      Using cn As New SqlConnection(DataBase.DbConn)
        cn.Open()
        Using cm As SqlCommand = cn.CreateCommand
          cm.CommandType = CommandType.StoredProcedure
          cm.CommandText = "getRoles"

          Using dr As New SafeDataReader(cm.ExecuteReader)
            With dr
              While .Read()
                Me.Add(Role.GetRole(dr))
              End While
              .Close()
            End With
          End Using
        End Using
        cn.Close()
      End Using

    End Sub

    <Transactional(TransactionalTypes.TransactionScope)> _
    Protected Overrides Sub DataPortal_Update()

      Using cn As New SqlConnection(DataBase.DbConn)
        cn.Open()
        For Each item As Role In DeletedList
          item.DeleteSelf(cn)
        Next
        DeletedList.Clear()

        For Each item As Role In Me
          If item.IsNew Then
            item.Insert(cn)

          Else
            item.Update(cn)
          End If
        Next
        cn.Close()
      End Using

    End Sub

#End Region

  End Class

End Namespace
