﻿Imports System.IO
Imports System.Reflection
Imports System.Xml
Imports InnerLibs

Namespace Locations

    ''' <summary>
    ''' Objeto que representa um estado do Brasil e seus respectivos detalhes
    ''' </summary>
    Public Class State

        ''' <summary>
        ''' Sigla do estado
        ''' </summary>
        ''' <returns></returns>
        Public Property Acronym As String

        ''' <summary>
        ''' Nome do estado
        ''' </summary>
        ''' <returns></returns>
        Public Property Name As String

        ''' <summary>
        ''' Região do Estado
        ''' </summary>
        ''' <returns></returns>
        Public Property Region As String

        ''' <summary>
        ''' Lista de cidades do estado
        ''' </summary>
        ''' <returns></returns>
        Public Property Cities As New List(Of String)

        ''' <summary>
        ''' Tipo de string representativa do estado (sigla ou nome)
        ''' </summary>
        Public Enum StateString
            Name
            Acronym
        End Enum

        ''' <summary>
        ''' inicializa um estado vazio
        ''' </summary>
        Public Sub New()

        End Sub

        ''' <summary>
        ''' Inicializa um objeto Estado a partir de uma sigla
        ''' </summary>
        ''' <param name="StateCode"></param>
        Public Sub New(StateCode As String)
            Me.Acronym = StateCode
            Me.Name = Brasil.GetNameOf(StateCode)
            Me.Cities = Brasil.GetCitiesOf(StateCode).ToList()
        End Sub

        ''' <summary>
        ''' Retorna a String correspondente ao estado
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return Acronym
        End Function

        ''' <summary>
        ''' Retorna a String correspondente ao estado
        ''' </summary>
        ''' <param name="Type">Tipo de String (Sigla ou Nome)</param>
        ''' <returns></returns>
        Public Overloads Function ToString(Optional Type As StateString = StateString.Acronym) As String
            Return If(Type = StateString.Name, Name, Acronym)
        End Function

    End Class

    ''' <summary>
    ''' Objeto para manipular cidades e estados do Brasil
    ''' </summary>
    Public NotInheritable Class Brasil

        ''' <summary>
        ''' Retorna uma lista com todos os estados do Brasil e seus respectivos detalhes
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property States As IEnumerable(Of State)
            Get
                Return CreateList()
            End Get
        End Property

        Private Shared l As List(Of State) = New List(Of State)
        Private Shared Function CreateList() As List(Of State)
            If Not l.Any() Then
                Dim r = New StreamReader([Assembly].GetExecutingAssembly().GetManifestResourceStream("InnerLibs.brasil.xml"))
                Dim s = r.ReadToEnd().ToString
                Dim doc = New XmlDocument()
                doc.LoadXml(s)
                For Each node As XmlNode In doc("brasil").ChildNodes
                    Dim estado = New State
                    estado.Acronym = node("Acronym").InnerText
                    estado.Name = node("Name").InnerText
                    estado.Region = node("Region").InnerText
                    For Each subnode As XmlNode In node("Cities").ChildNodes
                        estado.Cities.Add(subnode.InnerText)
                    Next
                    l.Add(estado)
                Next
            End If
            Return l
        End Function

        ''' <summary>
        ''' Retorna as Regiões dos estados brasileiros
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property Regions As IEnumerable(Of String)
            Get
                Return States.Select(Function(x) x.Region).Distinct().ToList()
            End Get
        End Property

        ''' <summary>
        ''' Retorna os estados de uma região
        ''' </summary>
        ''' <param name="Region"></param>
        ''' <returns></returns>
        Public Shared Function GetStatesOf(Optional Region As String = "", Optional Type As State.StateString = State.StateString.Name) As IEnumerable(Of String)
            Return States.Where(Function(x) x.Region = Region OrElse Region.IsBlank()).Select(Function(x) x.ToString(Type))
        End Function

        ''' <summary>
        ''' Retorna as cidades de um estado a partir do nome ou sigla do estado
        ''' </summary>
        ''' <param name="NameOrStateCode">Nome ou sigla do estado</param>
        ''' <returns></returns>
        Public Shared Function GetCitiesOf(Optional NameOrStateCode As String = "") As IEnumerable(Of String)
            Dim cities As New List(Of String)
            For Each estado As State In Brasil.States
                If estado.Acronym = NameOrStateCode Or estado.Name = NameOrStateCode Or NameOrStateCode.IsBlank() Then
                    cities.AddRange(estado.Cities)
                End If
            Next
            Return cities
        End Function

        ''' <summary>
        ''' Retorna uma lista contendo os nomes ou siglas dos estados do Brasil
        ''' </summary>
        ''' <param name="Type">Tipo de retorno (sigla ou nome)</param>
        ''' <returns></returns>
        Public Shared Function GetStateList(Optional Type As State.StateString = State.StateString.Name) As List(Of String)
            Dim StateCodes As New List(Of String)
            For Each es As State In States
                StateCodes.Add(es.ToString(Type))
            Next
            Return StateCodes
        End Function

        ''' <summary>
        ''' Retorna o nome do estado a partir da sigla
        ''' </summary>
        ''' <param name="StateCode"></param>
        ''' <returns></returns>
        Public Shared Function GetNameOf(StateCode As String) As String
            Dim name = ""
            For Each estado As State In Brasil.States
                If estado.Acronym = StateCode Then
                    name = estado.Name
                End If
            Next
            Return name
        End Function

        ''' <summary>
        ''' Retorna a Sigla a partir de um nome de estado
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <returns></returns>
        Public Shared Function GetAcronymOf(Name As String) As String
            Dim StateCode = ""
            For Each estado As State In Brasil.States
                If estado.Name = Name Then
                    StateCode = estado.Acronym
                End If
            Next
            Return StateCode
        End Function

    End Class

End Namespace