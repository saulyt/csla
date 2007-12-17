﻿Option Infer On

Imports System.Configuration
Imports System.Data.Linq

Namespace Data

  ''' <summary>
  ''' Provides an automated way to reuse 
  ''' LINQ data context objects within the context
  ''' of a single data portal operation.
  ''' </summary>
  ''' <typeparam name="C">
  ''' Type of database 
  ''' LINQ data context objects object to use.
  ''' </typeparam>
  ''' <remarks>
  ''' This type stores the LINQ data context object 
  ''' in <see cref="Csla.ApplicationContext.LocalContext" />
  ''' and uses reference counting through
  ''' <see cref="IDisposable" /> to keep the data context object
  ''' open for reuse by child objects, and to automatically
  ''' dispose the object when the last consumer
  ''' has called Dispose."
  ''' </remarks>
  Public Class ContextManager(Of C As {DataContext, New})

    Implements IDisposable

    Private Shared mLock As New Object
    Private mContext As C
    Private mConnectionString As String

    ''' <summary>
    ''' Gets the ContextManager object for the specified
    ''' key name.
    ''' </summary>
    ''' <param name="connectionString">
    ''' The database connection string.
    ''' </param>
    ''' <returns>ContextManager object for the name.</returns>
    Public Shared Function GetManager(ByVal connectionString As String) As ContextManager(Of C)

      SyncLock mLock
        Dim mgr As ContextManager(Of C)
        If ApplicationContext.LocalContext.Contains("__ctx:" & connectionString) Then
          mgr = CType(ApplicationContext.LocalContext("__ctx:" & connectionString), ContextManager(Of C))

        Else
          mgr = New ContextManager(Of C)(connectionString)
          ApplicationContext.LocalContext("__ctx:" & connectionString) = mgr
        End If
        mgr.AddRef()
        Return mgr
      End SyncLock

    End Function

    Private Sub New(ByVal connectionString As String)

      mConnectionString = connectionString

      mContext = DirectCast(Activator.CreateInstance(GetType(C), connectionString), C)

    End Sub

    ''' <summary>
    ''' Gets the LINQ data context object.
    ''' </summary>
    Public ReadOnly Property DataContext() As C
      Get
        Return mContext
      End Get
    End Property

#Region " Reference counting "

    Private mRefCount As Integer

    Private Sub AddRef()
      mRefCount += 1
    End Sub

    Private Sub DeRef()

      SyncLock mLock
        mRefCount -= 1
        If mRefCount = 0 Then
          mContext.Dispose()
          ApplicationContext.LocalContext.Remove("__ctx:" & mConnectionString)
        End If
      End SyncLock

    End Sub

#End Region

#Region " IDisposable "

    Public Sub Dispose() Implements IDisposable.Dispose
      DeRef()
    End Sub

#End Region

  End Class

End Namespace
