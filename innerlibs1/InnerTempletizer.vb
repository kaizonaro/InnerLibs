﻿
Imports System.Data.Common
Imports System.IO


Namespace Templatizer

    ''' <summary>
    ''' Gera HTML dinâmico a partir de uma conexão com banco de dados e um template HTML
    ''' </summary>
    Public NotInheritable Class TemplateBuilder

        ''' <summary>
        ''' Instancia um Novo TemplateBuilder utilizando uma Pasta para guardar os templates
        ''' </summary>
        ''' <param name="DataBase">Conexão com o banco de dados</param>
        ''' <param name="TemplateFolder">Pasta com os arquivos HTML dos templates</param>
        Sub New(DataBase As DataBase, TemplateFolder As DirectoryInfo)
            Me.TemplateFolder = TemplateFolder
            Me.DataBase = DataBase
        End Sub

        ''' <summary>
        ''' Instancia um Novo TemplateBuilder
        ''' </summary>
        ''' <param name="DataBase">Conexão com o banco de dados</param>
        ''' <param name="ApplicationAssembly">Assembly da aplicação onde os arquivos HTML dos templates estão compilados</param>


        Sub New(DataBase As DataBase, ApplicationAssembly As Reflection.Assembly)
            Me.DataBase = DataBase
            Me.ApplicationAssembly = ApplicationAssembly
        End Sub


        ''' <summary>
        ''' Conexão genérica de Banco de Dados
        ''' </summary>
        ''' <returns></returns>
        Property DataBase As DataBase

        ''' <summary>
        ''' Pasta contendo os arquivos HTML utilizados como template
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property TemplateFolder As DirectoryInfo = Nothing

        ''' <summary>
        ''' Aplicaçao contendo os Resources (arquivos compilados internamente) dos aruqivos HTML utilziados como template
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property ApplicationAssembly As Reflection.Assembly



        ''' <summary>
        ''' Processa os comandos SQL e retorna o resultado em HTML utilizando o arquivo de template especificado
        ''' </summary>
        ''' <param name="SQLQuery">Comando SQL</param>
        ''' <param name="TemplateFile">Arquivo do Template</param>
        ''' <returns></returns>    

        Public Function Build(Of Type)(SQLQuery As String, TemplateFile As String) As Type
            Dim response As Object

            Select Case GetType(Type)
                Case GetType(String)
                    response = ""
                Case GetType(List(Of String))
                    response = New List(Of String)
                Case Else
                    Throw New InvalidCastException("Only Type parameter can be only 'String' or 'List(Of String)'")
            End Select


            Dim template As String = ""
            Dim header As String = ""
            If IsNothing(ApplicationAssembly) Then
                Dim filefound = TemplateFolder.SearchFiles(SearchOption.TopDirectoryOnly, TemplateFile).First
                If Not filefound.Exists Then Throw New FileNotFoundException(TemplateFile.Quote & "  not found in " & TemplateFolder.Name)
                Using file As StreamReader = filefound.OpenText
                    template = file.ReadToEnd.RemoveNonPrintable.AdjustWhiteSpaces
                End Using
            Else
                Try
                    template = GetResourceFileText(ApplicationAssembly, ApplicationAssembly.GetName.Name & "." & TemplateFile).RemoveNonPrintable.AdjustWhiteSpaces
                Catch ex As Exception
                    Throw New FileNotFoundException(TemplateFile.Quote & "  not found in " & ApplicationAssembly.GetName.Name)
                End Try
            End If
            Try
                header = template.GetElementsByTagName("head").First.Content
            Catch ex As Exception
            End Try
            Try
                template = template.GetElementsByTagName("body").First.Content
            Catch ex As Exception
            End Try

            Using reader As DataBase.Reader = DataBase.RunSQL(SQLQuery)
                While reader.Read
                    Dim copia As String = template
                    'replace nas strings padrão
                    For Each col In reader.GetColumns
                        copia = copia.Replace("##" & col & "##", reader(col).ToString())
                    Next
                    'replace nas procedures
                    For Each sqlTag As HtmlTag In copia.GetElementsByTagName("sqlquery")
                        sqlTag.FixIn(copia)
                        Dim tp As String = Build(Of String)(sqlTag.Content, sqlTag.Attributes.Item("data-templatefile"))
                        copia = copia.Replace(sqlTag.ToString, tp)
                    Next
                    Select Case GetType(Type)
                        Case GetType(String)
                            response.Append(copia)
                        Case GetType(List(Of String))
                            response.add(copia)
                    End Select
                End While
            End Using
            Select Case GetType(Type)
                Case GetType(String)
                    response.Append(header)
                Case GetType(List(Of String))
                    response.Add(header)
            End Select
            Return response
        End Function

        Public Function GetTemplateList() As List(Of String)
            Dim l As New List(Of String)
            If IsNothing(ApplicationAssembly) Then
                For Each f In TemplateFolder.GetFiles
                    l.Add(f.Name)
                Next
            Else
                l.AddRange(ApplicationAssembly.GetManifestResourceNames())
            End If
            Return l
        End Function
    End Class
End Namespace
