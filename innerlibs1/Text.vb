﻿Imports System.Globalization
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Web
Imports System.Web.UI.HtmlControls
Imports System.Xml
Imports InnerLibs.HtmlParser
Imports InnerLibs.LINQ

''' <summary>
''' Modulo de manipulação de Texto
''' </summary>
''' <remarks></remarks>
Public Module Text



    ''' <summary>
    ''' Seprar uma string em varias partes a partir de varias strings removendo as entradas em branco
    ''' </summary>
    ''' <param name="Text"></param>
    ''' <param name="SplitText"></param>
    ''' <returns></returns>
    <Extension()> Public Function SplitAny(Text As String, ParamArray SplitText As String()) As String()
        SplitText = If(SplitText, {})
        Return Text.Split(SplitText, StringSplitOptions.RemoveEmptyEntries)
    End Function

    ''' <summary>
    ''' Substitui a ultima ocorrencia de um texto por outro
    ''' </summary>
    ''' <param name="Text"></param>
    ''' <param name="OldText"></param>
    ''' <param name="NewText"></param>
    ''' <returns></returns>
    <Extension()> Public Function ReplaceLast(Text As String, OldText As String, Optional NewText As String = "") As String
        If Text.Contains(OldText) Then
            Text = Text.Insert(Text.LastIndexOf(OldText), NewText)
            Text = Text.Remove(Text.LastIndexOf(OldText), 1)
        End If
        Return Text
    End Function

    ''' <summary>
    ''' Substitui a primeira ocorrencia de um texto por outro
    ''' </summary>
    ''' <param name="Text"></param>
    ''' <param name="OldText"></param>
    ''' <param name="NewText"></param>
    ''' <returns></returns>
    <Extension()> Public Function ReplaceFirst(Text As String, OldText As String, Optional NewText As String = "") As String
        If Text.Contains(OldText) Then
            Text = Text.Insert(Text.IndexOf(OldText), NewText)
            Text = Text.Remove(Text.IndexOf(OldText), 1)
        End If
        Return Text
    End Function

    ''' <summary>
    ''' Caracteres usado para encapsular palavras em textos
    ''' </summary>
    ''' <returns></returns>
    ReadOnly Property WordWrappers As String()
        Get
            Return OpenWrappers.Union(CloseWrappers).ToArray
        End Get
    End Property

    ReadOnly Property BreakLineChars As String() = {Environment.NewLine, vbCr, vbLf, vbCrLf, vbNewLine}

    ReadOnly Property CloseWrappers As String() = {"""", "'", ")", "}", "]", ">"}

    ReadOnly Property EndOfSentencePunctuation As String() = {".", "?", "!"}

    ReadOnly Property MidSentencePunctuation As String() = {":", ";", ","}

    ReadOnly Property OpenWrappers As String() = {"""", "'", "(", "{", "[", "<"}

    ''' <summary>
    ''' Caracteres em branco
    ''' </summary>
    ''' <returns></returns>
    ReadOnly Property WhiteSpaceChars As String() = {Environment.NewLine, " ", vbTab, vbLf, vbCr, vbCrLf}

    ''' <summary>
    ''' Strings utilizadas para descobrir as palavras em uma string
    ''' </summary>
    ''' <returns></returns>
    ReadOnly Property WordSplitters As String() = {"&nbsp;", """", "'", "(", ")", ",", ".", "?", "!", ";", "{", "}", "[", "]", "|", " ", ":", vbNewLine, "<br>", "<br/>", "<br />", Environment.NewLine, vbCr, vbCrLf}

    <Extension()>
    Public Function AdjustBlankSpaces(ByVal Text As String) As String
        Return AdjustWhiteSpaces(Text)
    End Function

    <Extension()>
    Public Function AdjustWhiteSpaces(ByVal Text As String) As String
        Text = Text.IfBlank("")
        If Text.IsNotBlank Then

            'adiciona espaco quando nescessario
            Text = Text.Replace(")", ") ")
            Text = Text.Replace("]", "] ")
            Text = Text.Replace("}", "} ")
            Text = Text.Replace(">", "> ")
            Text = Text.Replace("(", " (")
            Text = Text.Replace("<", " <")
            Text = Text.Replace("[", " [")
            Text = Text.Replace("{", " {")
            Text = Text.Replace(":", ": ")
            Text = Text.Replace(";", "; ")
            Text = Text.Replace("""", " """)

            'remove espaco quando nescessario
            Text = Text.Replace(" ,", ",")
            Text = Text.Replace(" .", ".")
            Text = Text.Replace(" !", "!")
            Text = Text.Replace(" ?", "?")
            Text = Text.Replace(" ;", ";")
            Text = Text.Replace(" :", ":")
            Text = Text.Replace(" )", ")")
            Text = Text.Replace(" ]", "]")
            Text = Text.Replace(" }", "}")
            Text = Text.Replace(" >", ">")
            Text = Text.Replace("( ", "(")
            Text = Text.Replace("[ ", "[")
            Text = Text.Replace("{ ", "{")
            Text = Text.Replace("< ", "<")
            Text = Text.Replace(""" ", """")

            Dim arr = Text.Split(New String() {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
            Text = arr.Join(Environment.NewLine)
            arr = Text.Split(New String() {" "}, StringSplitOptions.RemoveEmptyEntries)
            Text = arr.Join(" ")

        End If

        Return Text.TrimAny(" ", Environment.NewLine)

    End Function

    ''' <summary>
    ''' Retorna uma string em ordem afabética baseada em uma outra string
    ''' </summary>
    ''' <param name="Text"></param>
    ''' <returns></returns>
    <Extension()>
    Public Function Alphabetize(Text As String) As String
        Dim a = Text.ToCharArray()
        Array.Sort(a)
        Return a.Join("")
    End Function


    <Extension()>
    Public Function AppendUrlParameter(ByRef Url As String, Key As String, ParamArray Value As String()) As String
        For Each v In If(Value, {})
            Url.Append(String.Format("&{0}={1}", Key, v.IfBlank("")))
        Next
        Return Url
    End Function

    ''' <summary>
    ''' Adiciona texto ao final de uma string
    ''' </summary>
    ''' <param name="Text">      Texto</param>
    ''' <param name="AppendText">Texto adicional</param>
    <Extension()>
    Public Function Append(ByRef Text As String, AppendText As String) As String
        Text &= AppendText
        Return Text
    End Function

    ''' <summary>
    ''' Adiciona texto ao final de uma string se um criterio for cumprido
    ''' </summary>
    ''' <param name="Text">      Texto</param>
    ''' <param name="AppendText">Texto adicional</param>
    ''' <param name="Test">      Teste</param>
    <Extension()> Public Function AppendIf(ByRef Text As String, AppendText As String, Test As Boolean) As String
        If Test Then
            Text.Append(AppendText)
        End If
        Return Text
    End Function

    ''' <summary>
    ''' Adiciona texto ao final de uma string se um criterio for cumprido
    ''' </summary>
    ''' <param name="Text">      Texto</param>
    ''' <param name="AppendText">Texto adicional</param>
    ''' <param name="Test">      Teste</param>
    <Extension()> Public Function AppendIf(ByRef Text As String, AppendText As String, Test As Func(Of String, Boolean)) As String
        Test = If(Test, Function(x) False)
        Return Text.AppendIf(AppendText, Test(Text))
    End Function

    ''' <summary>
    ''' Adiciona texto ao final de uma string se um criterio for cumprido
    ''' </summary>
    ''' <param name="Text">      Texto</param>
    ''' <param name="AppendText">Texto adicional</param>
    ''' <param name="Test">      Teste</param>
    <Extension()> Public Function PrependIf(ByRef Text As String, AppendText As String, Test As Func(Of String, Boolean)) As String
        Test = If(Test, Function(x) False)
        Return Text.PrependIf(AppendText, Test(Text))
    End Function

    ''' <summary>
    ''' Aplica espacos em todos os caracteres de encapsulamento
    ''' </summary>
    ''' <param name="Text"></param>
    ''' <returns></returns>
    <Extension> Function ApplySpaceOnWrapChars(Text As String) As String
        For Each c In WordWrappers
            Text = Text.Replace(c, " " & c & " ")
        Next
        Return Text
    End Function

    ''' <summary>
    ''' Transforma uma palavra em CameCase em varias palavras a partir de suas letras maíusculas
    ''' </summary>
    ''' <param name="Text"></param>
    ''' <returns></returns>
    <Extension()> Public Function CamelSplit(Text As String) As String
        Dim chars = Text.ToArray
        Text = ""
        For Each c In chars
            If Char.IsUpper(c) Then
                Text &= " "
            End If
            Text &= c
        Next
        Return Text.Trim
    End Function

    ''' <summary>
    ''' Censura as palavras de um texto substituindo as palavras indesejadas por * (ou outro
    ''' caractere desejado) e retorna um valor indicando se o texto precisou ser censurado
    ''' </summary>
    ''' <param name="Text">               Texto</param>
    ''' <param name="BadWords">           Lista de palavras indesejadas</param>
    ''' <param name="CensorshipCharacter">Caractere que será aplicado nas palavras censuradas</param>
    ''' <returns>TRUE se a frase precisou ser censurada, FALSE se a frase não precisou de censura</returns>
    <Extension()>
    Public Function Censor(ByRef Text As String, BadWords As IEnumerable(Of String), Optional CensorshipCharacter As Char = "*") As Boolean
        Dim IsCensored As Boolean = False
        Dim words As String() = Text.Split(" ", StringSplitOptions.None)
        If words.ContainsAny(BadWords) Then
            For Each bad In BadWords
                Dim censored = ""
                For index = 1 To bad.Length
                    censored.Append(CensorshipCharacter)
                Next
                For index = 0 To words.Length - 1
                    If words(index).RemoveDiacritics.RemoveAny(WordSplitters).ToLower = bad.RemoveDiacritics.RemoveAny(WordSplitters).ToLower Then
                        words(index) = words(index).ToLower().Replace(bad, censored)
                        IsCensored = True
                    End If
                Next
            Next
            Text = words.Join(" ")
        End If
        Return IsCensored
    End Function

    ''' <summary>
    ''' Retorna um novo texto censurando as palavras de um texto substituindo as palavras indesejadas
    ''' por um caractere desejado)
    ''' </summary>
    ''' <param name="Text">               Texto</param>
    ''' <param name="BadWords">           Array de palavras indesejadas</param>
    ''' <param name="CensorshipCharacter">Caractere que será aplicado nas palavras censuradas</param>
    <Extension()>
    Public Function Censor(ByVal Text As String, CensorshipCharacter As Char, ParamArray BadWords As String()) As String
        Dim txt As String = Text
        txt.Censor(BadWords.ToList, CensorshipCharacter)
        Return txt
    End Function

    ''' <summary>
    ''' Verifica se uma string contém a maioria dos valores especificados
    ''' </summary>
    ''' <param name="Text">  Texto correspondente</param>
    ''' <param name="Values">Lista de valores</param>
    ''' <returns>True se conter todos os valores, false se não</returns>
    <Extension>
    Public Function ContainsMost(Text As String, ComparisonType As StringComparison, ParamArray Values As String()) As Boolean
        Values = If(Values, {})
        Dim l As New List(Of Boolean)
        If Values.Count > 0 Then
            For Each value In Values
                l.Add(Not IsNothing(Text) AndAlso Text.IndexOf(value, ComparisonType) <> -1)
            Next
        End If
        Return l.Most
    End Function

    ''' <summary>
    ''' Verifica se uma string contém a maioria dos valores especificados
    ''' </summary>
    ''' <param name="Text">  Texto correspondente</param>
    ''' <param name="Values">Lista de valores</param>
    ''' <returns>True se conter todos os valores, false se não</returns>
    <Extension>
    Public Function ContainsMost(Text As String, ParamArray Values As String()) As Boolean
        Return Text.ContainsMost(StringComparison.CurrentCultureIgnoreCase, Values)
    End Function


    ''' <summary>
    ''' Verifica se uma String contém todos os valores especificados
    ''' </summary>
    ''' <param name="Text">  Texto correspondente</param>
    ''' <param name="Values">Lista de valores</param>
    ''' <returns>True se conter todos os valores, false se não</returns>
    <Extension>
    Public Function ContainsAll(Text As String, ParamArray Values As String()) As Boolean
        Return Text.ContainsAll(StringComparison.CurrentCultureIgnoreCase, Values)
    End Function

    ''' <summary>
    ''' Verifica se uma String contém todos os valores especificados
    ''' </summary>
    ''' <param name="Text">          Texto correspondente</param>
    ''' <param name="Values">        Lista de valores</param>
    ''' <param name="ComparisonType">Tipo de comparacao</param>
    ''' <returns>True se conter algum valor, false se não</returns>
    <Extension>
    Public Function ContainsAll(Text As String, ComparisonType As StringComparison, ParamArray Values As String()) As Boolean
        Values = If(Values, {})
        If Values.Count > 0 Then
            For Each value As String In Values
                If IsNothing(Text) OrElse Text.IndexOf(value, ComparisonType) = -1 Then
                    Return False
                End If
            Next
            Return True
        End If
        Return Text.IsBlank
    End Function

    ''' <summary>
    ''' Verifica se uma String contém qualquer um dos valores especificados
    ''' </summary>
    ''' <param name="Text">  Texto correspondente</param>
    ''' <param name="Values">Lista de valores</param>
    ''' <returns>True se conter algum valor, false se não</returns>
    <Extension>
    Public Function ContainsAny(Text As String, ParamArray Values As String()) As Boolean
        Return Text.ContainsAny(StringComparison.CurrentCultureIgnoreCase, Values)
    End Function

    ''' <summary>
    ''' Verifica se uma String contém qualquer um dos valores especificados
    ''' </summary>
    ''' <param name="Text">          Texto correspondente</param>
    ''' <param name="Values">        Lista de valores</param>
    ''' <param name="ComparisonType">Tipo de comparacao</param>
    ''' <returns>True se conter algum valor, false se não</returns>
    <Extension>
    Public Function ContainsAny(Text As String, ComparisonType As StringComparison, ParamArray Values As String()) As Boolean
        Values = If(Values, {})
        If Values.Count > 0 Then
            For Each value As String In If(Values, {})
                If Not IsNothing(Text) AndAlso Text.IndexOf(value, ComparisonType) <> -1 Then
                    Return True
                End If
            Next
            Return False
        Else
            Return Text.IsNotBlank
        End If
    End Function

    ''' <summary>
    ''' Conta os caracters especificos de uma string
    ''' </summary>
    ''' <param name="Text">     Texto</param>
    ''' <param name="Character">Caractere</param>
    ''' <returns></returns>
    <Extension>
    Public Function CountCharacter(ByVal Text As String, ByVal Character As Char) As Integer
        Return Text.Count(Function(c As Char) c = Character)
    End Function

    ''' <summary>
    ''' Retorna as plavaras contidas em uma frase em ordem alfabética e sua respectiva quantidade
    ''' </summary>
    ''' <param name="Text">            TExto</param>
    ''' <param name="RemoveDiacritics">indica se os acentos devem ser removidos das palavras</param>
    ''' <param name="Words">           
    ''' Desconsidera outras palavras e busca a quantidadade de cada palavra especificada em um array
    ''' </param>
    ''' <returns></returns>
    <Extension()> Function CountWords(Text As String, Optional RemoveDiacritics As Boolean = True, Optional Words As String() = Nothing) As Dictionary(Of String, Long)
        If Words Is Nothing Then Words = {}
        Dim palavras = Text.Split(WordSplitters, StringSplitOptions.RemoveEmptyEntries).ToArray

        If Words.Count > 0 Then
            palavras = palavras.Where(Function(x) Words.Select(Function(y) y.ToLower).Contains(x.ToLower)).ToArray
        End If

        If RemoveDiacritics Then
            palavras = palavras.Select(Function(p) p.RemoveDiacritics).ToArray
            Words = Words.Select(Function(p) p.RemoveDiacritics).ToArray
        End If

        Dim dic As Dictionary(Of String, Long) = palavras.DistinctCount()

        For Each w In Words.Where(Function(x) Not dic.Keys.Contains(x))
            dic.Add(w, 0)
        Next
        Return dic
    End Function

    ''' <summary>
    ''' Cria um elemento HTML a partir de uma string HTML
    ''' </summary>
    ''' <param name="HTMLString">String contendo o HTML</param>
    ''' <returns></returns>
    <Extension()>
    Function CreateGenericElement(HTMLString As String) As HtmlGenericControl
        Return New HtmlParser.HtmlElement(HTMLString).CreateWebFormControl(Of HtmlGenericControl)
    End Function

    ''' <summary>
    ''' Decrementa em 1 ou mais um numero inteiro
    ''' </summary>
    ''' <param name="Number">Numero</param>
    ''' <param name="Amount">QUantidade que será removida</param>
    <Extension()>
    Public Function Decrement(ByRef Number As Integer, Optional Amount As Integer = 1) As Integer
        Number = Number - Amount.SetMinValue(1)
        Return Number
    End Function

    ''' <summary>
    ''' Decrementa em 1 ou mais um numero inteiro
    ''' </summary>
    ''' <param name="Number">Numero</param>
    ''' <param name="Amount">QUantidade que será removida</param>
    <Extension()>
    Public Function Decrement(ByRef Number As Long, Optional Amount As Integer = 1) As Integer
        Number = Number - Amount.SetMinValue(1)
        Return Number
    End Function

    ''' <summary>
    ''' Remove uma linha especifica de um texto
    ''' </summary>
    ''' <param name="Text">Texto</param>
    ''' <param name="LineIndex">Numero da linha</param>
    ''' <returns></returns>
    <Extension()>
    Public Function DeleteLine(ByVal Text As String, LineIndex As Integer) As String
        Dim parts As New List(Of String)
        Dim strReader = New StringReader(Text)
        Dim NewText As String = ""
        LineIndex = LineIndex.SetMinValue(0)
        While True
            NewText = strReader.ReadLine()
            If NewText Is Nothing Then
                Exit While
            Else
                parts.Add(NewText)
            End If
        End While
        NewText = ""
        If parts.Count > LineIndex Then
            parts.RemoveAt(LineIndex)
        End If
        For Each part In parts
            NewText = NewText & part & Environment.NewLine
        Next
        Return NewText
    End Function

    ''' <summary>
    ''' Cria um dicionário com as palavras de uma lista e a quantidade de cada uma.
    ''' </summary>
    ''' <param name="List">Lista de palavras</param>
    ''' <returns></returns>
    Public Function DistinctCount(ParamArray List As String()) As Dictionary(Of String, Long)
        Return List.ToList.DistinctCount
    End Function

    ''' <summary>
    ''' Cria um dicionário com as palavras de uma frase e sua respectiva quantidade.
    ''' </summary>
    ''' <param name="Phrase">Lista de palavras</param>
    ''' <returns></returns>
    <Extension()> Function DistinctCount(Phrase As String) As Dictionary(Of String, Long)
        Return Phrase.Split(" ").ToList.DistinctCount
    End Function

    ''' <summary>
    ''' Verifica se uma string termina com alguma outra string de um array
    ''' </summary>
    ''' <param name="Text"> </param>
    ''' <param name="Words"></param>
    ''' <returns></returns>
    <Extension()> Function EndsWithAny(Text As String, ParamArray Words As String()) As Boolean
        Return Words.Any(Function(p) Text.EndsWith(p))
    End Function

    ''' <summary>
    ''' Prepara uma string com aspas simples para uma Query TransactSQL
    ''' </summary>
    ''' <param name="Text">Texto a ser tratado</param>
    ''' <returns>String pornta para a query</returns>
    <Extension()>
    Public Function EscapeQuotesToQuery(Text As String) As String
        Return Text.Replace("'", "''")
    End Function

    ''' <summary>
    ''' Procura numeros em uma string e retorna um array deles
    ''' </summary>
    ''' <param name="Text"></param>
    ''' <returns></returns>
    <Extension()> Public Function FindNumbers(Text As String) As Decimal()
        Dim l As New List(Of Decimal)
        Dim numbers As String() = Regex.Split(Text, "\D+")
        For Each value In numbers
            If Not value.IsBlank Then
                l.Add(value.ChangeType(Of Decimal))
            End If
        Next
        Return l.ToArray
    End Function

    ''' <summary>
    ''' Procurea numeros de telefone em um texto
    ''' </summary>
    ''' <param name="Text"></param>
    ''' <returns></returns>
    <Extension()> Public Function FindTelephoneNumbers(Text As String) As List(Of String)
        Dim tels As New List(Of String)
        For Each m As Match In New Regex("\b[\s()\d-]{6,}\d\b", RegexOptions.Singleline + RegexOptions.IgnoreCase).Matches(Text)
            tels.Add(m.Value.ToTelephone)
        Next
        Return tels
    End Function

    ''' <summary>
    ''' Transforma quebras de linha HTML em quebras de linha comuns ao .net
    ''' </summary>
    ''' <param name="Text">Texto correspondente</param>
    ''' <returns>String fixada</returns>
    <Extension>
    Public Function FixBreakLines(Text As String) As String
        Return Text.ReplaceMany(vbCr & vbLf, "<br/>", "<br />", "<br>")
        Return Text.Replace("&nbsp;", " ")
    End Function

    <Extension()>
    Public Function FixCaptalization(ByVal Text As String) As String
        Text = Text.Trim().GetFirstChars(1).ToUpper() & Text.RemoveFirstChars(1)
        Dim dots As String() = {"...", ". ", "? ", "! "}
        Dim sentences As List(Of String)
        For Each dot In dots
            sentences = Text.Split(dot).ToList
            For index = 0 To sentences.Count - 1
                sentences(index) = "" & sentences(index).Trim().GetFirstChars(1).ToUpper() & sentences(index).RemoveFirstChars(1)
            Next
            Text = sentences.Join(dot)
        Next

        sentences = Text.Split(" ").ToList
        Text = ""
        For Each c In sentences
            Dim palavra = c
            If palavra.EndsWith(".") AndAlso palavra.Length = 2 Then
                palavra = palavra.ToUpper
                Text.Append(palavra)

                Dim proximapalavra = sentences.IfNoIndex(sentences.IndexOf(c) + 1, "")
                If Not (proximapalavra.EndsWith(".") AndAlso palavra.Length = 2) Then
                    Text.Append(" ")
                End If
            Else
                Text.Append(c & " ")
            End If
        Next

        Return Text.RemoveLastChars(1)
    End Function

    ''' <summary>
    ''' Adciona pontuaçao ao final de uma string se a mesma não terminar com alguma pontuacao.
    ''' </summary>
    ''' <param name="Text">       Frase, Texto a ser pontuado</param>
    ''' <param name="Punctuation">Ponto a ser adicionado na frase se a mesma não estiver com pontuacao</param>
    ''' <returns>Frase corretamente pontuada</returns>
    <Extension>
    Public Function FixPunctuation(ByRef Text As String, Optional Punctuation As String = ".", Optional ForceSpecificPunctuation As Boolean = False) As String
        Text = Text.RemoveLastAny(True, ",", " ")
        Dim pts = {".", "!", "?", ":", ";"}
        If ForceSpecificPunctuation Then
            Text = Text.RemoveLastAny(True, pts).Trim & Punctuation
        Else
            If Not Text.EndsWithAny(pts) Then
                Text = Text & Punctuation
            End If
        End If
        Return Text
    End Function

    ''' <summary>
    ''' Arruma a ortografia do texto captalizando corretamente, adcionando pontução ao final de frase
    ''' caso nescessário e removendo espaços excessivos ou incorretos
    ''' </summary>
    ''' <param name="Text">Texto</param>
    ''' <returns></returns>
    <Extension()> Public Function FixText(ByVal Text As String) As String
        Return New StructuredText(Text).ToString
    End Function

    ''' <summary>
    ''' Extension Method para <see cref="String.Format(String,Object())"/>
    ''' </summary>
    ''' <param name="Text">Texto</param>
    ''' <param name="Args">Objetos de substituição</param>
    ''' <returns></returns>
    <Extension> Public Function Format(Text As String, ParamArray Args As String()) As String
        Return String.Format(Text, Args)
    End Function

    ''' <summary>
    ''' Retorna um texto posterior a outro
    ''' </summary>
    ''' <param name="Text"> Texto correspondente</param>
    ''' <param name="Value">Texto Posterior</param>
    ''' <returns>Uma string com o valor posterior ao valor especificado.</returns>
    <Extension>
    Public Function GetAfter(Text As String, Value As String) As String
        If IsNothing(Value) Then Value = ""
        If IsNothing(Text) OrElse Text.IndexOf(Value) = -1 Then
            Return "" & Text
        End If
        Return Text.Substring(Text.IndexOf(Value) + Value.Length)
    End Function

    ''' <summary>
    ''' Retorna todas as ocorrencias de um texto entre dois textos
    ''' </summary>
    ''' <param name="Text">  O texto correspondente</param>
    ''' <param name="Before">O texto Anterior</param>
    ''' <param name="After"> O texto Posterior</param>
    ''' <returns>Uma String com o texto entre o texto anterior e posterior</returns>
    <Extension()> Public Function GetAllBetween(Text As String, Before As String, Optional After As String = "") As String()
        Dim lista As New List(Of String)
        Dim regx = Before.RegexEscape & "(.*?)" & After.IfBlank(Of String)(Before).RegexEscape
        Dim mm = New Regex(regx, RegexOptions.Singleline + RegexOptions.IgnoreCase).Matches(Text)
        For Each a As Match In mm
            lista.Add(a.Value.RemoveFirstIf(Before).RemoveLastIf(After))
        Next
        Return lista.ToArray
    End Function

    ''' <summary>
    ''' Retorna um texto anterior a outro
    ''' </summary>
    ''' <param name="Text"> Texto correspondente</param>
    ''' <param name="Value">Texto Anterior</param>
    ''' <returns>Uma string com o valor anterior ao valor especificado.</returns>
    <Extension>
    Public Function GetBefore(Text As String, Value As String) As String
        If IsNothing(Value) Then Value = ""
        If IsNothing(Text) OrElse Text.IndexOf(Value) = -1 Then
            Return "" & Text
        End If
        Return Text.Substring(0, Text.IndexOf(Value))
    End Function

    ''' <summary>
    ''' Retorna o texto entre dois textos
    ''' </summary>
    ''' <param name="Text">  O texto correspondente</param>
    ''' <param name="Before">O texto Anterior</param>
    ''' <param name="After"> O texto Posterior</param>
    ''' <returns>Uma String com o texto entre o texto anterior e posterior</returns>
    <Extension>
    Public Function GetBetween(Text As String, Before As String, After As String) As String
        If Text.IsBlank() Then
            Return ""
        End If
        Dim beforeStartIndex As Integer = Text.IndexOf(Before)
        Dim startIndex As Integer = beforeStartIndex + Before.Length
        Dim afterStartIndex As Integer = Text.IndexOf(After, startIndex)

        If beforeStartIndex < 0 OrElse afterStartIndex < 0 Then
            Return Text
        End If

        Return Text.Substring(startIndex, afterStartIndex - startIndex)
    End Function

    ''' <summary>
    ''' Pega o dominio principal de uma URL
    ''' </summary>
    ''' <param name="URL">URL</param>
    ''' <returns>nome do dominio</returns>
    <Extension>
    Public Function GetDomain(URL As Uri, Optional RemoveFirstSubdomain As Boolean = False) As String
        Dim d = URL.GetLeftPart(UriPartial.Authority).RemoveAny("http://", "https://", "www.")
        If RemoveFirstSubdomain Then
            Dim parts = d.Split(".").ToList
            parts.Remove(parts.Item(0))
            d = parts.Join(".")
        End If
        Return d
    End Function

    ''' <summary>
    ''' Pega o dominio principal de uma URL
    ''' </summary>
    ''' <param name="URL">URL</param>
    ''' <returns>nome do dominio</returns>
    <Extension>
    Public Function GetDomain(URL As String, Optional RemoveFirstSubdomain As Boolean = False) As String
        Return New Uri(URL).GetDomain(RemoveFirstSubdomain)
    End Function

    <Extension()>
    Public Function GetFirstChars(Text As String, Optional Number As Integer = 1) As String
        If Text.Length < Number Or Number < 1 Then
            Return Text
        Else
            Return Text.Substring(0, Number)
        End If

    End Function

    ''' <summary>
    ''' Extrai palavras chave de um texto seguindo critérios especificos.
    ''' </summary>
    ''' <param name="TextOrURL">       Texto principal ou URL</param>
    ''' <param name="MinWordCount">    Minimo de aparições da palavra no texto</param>
    ''' <param name="MinWordLenght">   Tamanho minimo da palavra</param>
    ''' <param name="IgnoredWords">    palavras que sempre serão ignoradas</param>
    ''' <param name="RemoveDiacritics">TRUE para remover acentos</param>
    ''' <param name="ImportantWords">  
    ''' Palavras importantes. elas sempre serão adicionadas a lista de tags desde que não estejam nas <paramref name="IgnoredWords"/>
    ''' </param>
    ''' <returns></returns>
    <Extension()> Function GetKeyWords(TextOrURL As String, Optional MinWordCount As Integer = 1, Optional MinWordLenght As Integer = 1, Optional LimitCollection As Integer = 0, Optional RemoveDiacritics As Boolean = True, Optional ByVal IgnoredWords As String() = Nothing, Optional ImportantWords As String() = Nothing) As Dictionary(Of String, Long)
        Dim l As New List(Of String)

        Dim tg As String = ""
        Dim doc As HtmlDocument
        Try
            doc = New HtmlDocument(TextOrURL)

            Dim kw = doc.FindElements(Function(e As HtmlParser.HtmlElement) e.Name = "meta" AndAlso e.Attribute("name").ToLower = "keywords")
            For Each item As HtmlParser.HtmlElement In kw
                l.AddRange(item.Attribute("content").Split(","))
            Next

            For Each node As HtmlParser.HtmlElement In doc.Nodes.GetElementsByTagName("style", True)
                node.Destroy()
            Next

            For Each node As HtmlParser.HtmlElement In doc.Nodes.GetElementsByTagName("script", True)
                node.Destroy()
            Next

            For Each node As HtmlParser.HtmlElement In doc.Nodes.GetElementsByTagName("head", True)
                node.Destroy()
            Next

            If doc.Nodes.GetElementsByTagName("article", True).Count > 0 Then
                TextOrURL = CType(doc.Nodes.GetElementsByTagName("article", True)(0), HtmlParser.HtmlElement).InnerHTML
            ElseIf doc.Nodes.GetElementsByTagName("body", True).Count > 0 Then
                TextOrURL = CType(doc.Nodes.GetElementsByTagName("body", True)(0), HtmlParser.HtmlElement).InnerHTML
            Else
                'texto limpo
            End If

            'comeca a extrair as palavras por quantidade
            Dim palavras = TextOrURL.FixBreakLines.RemoveHTML.CountWords(RemoveDiacritics).Where(Function(p) Not p.IsNumber).ToArray
            IgnoredWords = If(IgnoredWords, {}).ToArray
            ImportantWords = If(ImportantWords, {}).Where(Function(p) Not p.IsIn(IgnoredWords)).ToArray
            l.AddRange(ImportantWords)
            ImportantWords = l.ToArray

            If RemoveDiacritics Then
                IgnoredWords = IgnoredWords.Select(Function(p) p.RemoveDiacritics).ToArray
                ImportantWords = ImportantWords.Select(Function(p) p.RemoveDiacritics).ToArray
            End If

            palavras = palavras.Where(Function(p) p.Key.IsIn(ImportantWords)).Union(palavras.Where(Function(p) p.Key.Length >= MinWordLenght).Where(Function(p) p.Value >= MinWordCount).Where(Function(p) Not IgnoredWords.Contains(p.Key)).Take(If(LimitCollection < 1, palavras.Count, LimitCollection))).Distinct().OrderByDescending(Function(p) p.Value).ToArray
            Return palavras.ToDictionary(Function(p) p.Key, Function(p) p.Value)
        Catch ex As Exception
            Debug.Write(ex)
            Return New Dictionary(Of String, Long)
        End Try
    End Function

    <Extension()>
    Public Function GetLastChars(Text As String, Optional Number As Integer = 1) As String
        If Text.Length < Number Or Number < 1 Then
            Return Text
        Else
            Return Text.Substring(Text.Length - Number)
        End If
    End Function

    ''' <summary>
    ''' Retorna N caracteres de uma string a partir do caractere encontrado no centro
    ''' </summary>
    ''' <param name="Text">  </param>
    ''' <param name="Length"></param>
    ''' <returns></returns>
    <Extension()> Public Function GetMiddleChars(ByVal Text As String, Length As Integer) As String
        Text = Text.IfBlank("")
        If Text.Length >= Length Then
            If Text.Length Mod 2 <> 0 Then
                Try
                    Return Text.Substring(Text.Length / 2 - 1, Length)
                Catch ex As Exception
                    Return Text.GetMiddleChars(Length - 1)
                End Try
            Else
                Return Text.RemoveLastChars(1).GetMiddleChars(Length)
            End If
        End If
        Return Text
    End Function

    ''' <summary>
    ''' Retorna o caractere de encapsulamento oposto ao caractere indicado
    ''' </summary>
    ''' <param name="Text">Caractere</param>
    ''' <returns></returns>
    <Extension> Function GetOppositeWrapChar(Text As String) As String
        Select Case Text.GetFirstChars()
            Case """"
                Return """"
            Case "'"
                Return "'"
            Case "("
                Return ")"
            Case ")"
                Return "("
            Case "["
                Return "]"
            Case "]"
                Return "["
            Case "{"
                Return "}"
            Case "}"
                Return "{"
            Case "<"
                Return ">"
            Case ">"
                Return "<"
            Case "\"
                Return "/"
            Case "/"
                Return "\"
            Case "¿"
                Return "?"
            Case "?"
                Return "¿"
            Case "!"
                Return "¡"
            Case "¡"
                Return "!"
            Case "."
                Return "."
            Case ":"
                Return ":"
            Case ";"
                Return ";"
            Case "_"
                Return "_"
            Case Else
                Return Text
        End Select
    End Function

    ''' <summary>
    ''' Sorteia um item da Lista
    ''' </summary>
    ''' <typeparam name="Type">Tipo de lista</typeparam>
    ''' <param name="List">Lista</param>
    ''' <returns>Um valor do tipo especificado</returns>
    <Extension>
    Public Function GetRandomItem(Of Type)(ByVal List As List(Of Type)) As Type
        Return List.ToArray.GetRandomItem
    End Function

    ''' <summary>
    ''' Sorteia um item da Lista
    ''' </summary>
    ''' <typeparam name="Type">Tipo da Matriz</typeparam>
    ''' <param name="Array">Matriz</param>
    ''' <returns>Um valor do tipo especificado</returns>
    <Extension>
    Public Function GetRandomItem(Of Type)(ByVal Array As Type()) As Type
        Return Array.Shuffle()(0)
    End Function

    ''' <summary>
    ''' Retorna o caminho relativo da url
    ''' </summary>
    ''' <param name="URL">Url</param>
    ''' <returns></returns>
    <Extension()>
    Public Function GetRelativeURL(URL As Uri) As String
        Return URL.PathAndQuery.RemoveFirstIf(URL.AbsoluteUri)
    End Function

    ''' <summary>
    ''' Retorna o caminho relativo da url
    ''' </summary>
    ''' <param name="URL">Url</param>
    ''' <returns></returns>
    <Extension()>
    Public Function GetRelativeURL(URL As String) As String
        Return New Uri(URL).GetRelativeURL()
    End Function

    ''' <summary>
    ''' Retorna uma lista de palavras encontradas no texto em ordem alfabetica
    ''' </summary>
    ''' <param name="Text"></param>
    ''' <returns></returns>
    <Extension()> Function GetWords(Text As String) As IOrderedEnumerable(Of String)
        Dim txt As New List(Of String)
        Dim palavras As List(Of String) = Text.AdjustWhiteSpaces.FixBreakLines.ToLower.RemoveHTML.Split(WordSplitters, StringSplitOptions.RemoveEmptyEntries).ToList
        For Each w In palavras
            txt.Add(w)
        Next
        Return txt.Distinct.OrderBy(Function(x) x)
    End Function

    ''' <summary>
    ''' Captura todas as sentenças que estão entre aspas ou parentesis ou chaves ou colchetes em um texto
    ''' </summary>
    ''' <param name="Text">Texto</param>
    ''' <returns></returns>
    <Extension> Public Function GetWrappedText(Text As String, Optional Character As String = """", Optional ExcludeWrapChars As Boolean = True) As String()
        Dim lista As New List(Of String)
        Dim regx = Character.RegexEscape & "(.*?)" & Character.ToString.GetOppositeWrapChar.RegexEscape
        Dim mm = New Regex(regx, RegexOptions.Singleline + RegexOptions.IgnoreCase).Matches(Text)
        For Each a As Match In mm
            If ExcludeWrapChars Then
                lista.Add(a.Value.RemoveFirstIf(Character).RemoveLastIf(GetOppositeWrapChar(Character)))
            Else
                lista.Add(a.Value)
            End If
        Next
        Return lista.ToArray
    End Function

    ''' <summary>
    ''' Retorna um texto com entidades HTML convertidas para caracteres e tags BR em breaklines
    ''' </summary>
    ''' <param name="Text">string HTML</param>
    ''' <returns>String HTML corrigido</returns>
    <Extension()>
    Public Function HtmlDecode(ByVal Text As String) As String
        Return System.Web.HttpUtility.HtmlDecode("" & Text).ReplaceMany(vbCr & vbLf, "<br/>", "<br />", "<br>")
    End Function

    ''' <summary>
    ''' Escapa o texto HTML
    ''' </summary>
    ''' <param name="Text">string HTML</param>
    ''' <returns>String HTML corrigido</returns>
    <Extension()>
    Public Function HtmlEncode(ByVal Text As String) As String
        Return System.Web.HttpUtility.HtmlEncode("" & Text.ReplaceMany("<br>", BreakLineChars))
    End Function

    ''' <summary>
    ''' Incrementa em 1 ou mais um numero inteiro
    ''' </summary>
    ''' <param name="Number">Numero</param>
    ''' <param name="Amount">QUantidade adicionada</param>
    <Extension()>
    Public Function Increment(ByRef Number As Integer, Optional Amount As Integer = 1) As Integer
        Number = Number + Amount.SetMinValue(1)
        Return Number
    End Function

    ''' <summary>
    ''' Incrementa em 1 ou mais um numero inteiro
    ''' </summary>
    ''' <param name="Number">Numero</param>
    ''' <param name="Amount">QUantidade adicionada</param>
    <Extension()>
    Public Function Increment(ByRef Number As Long, Optional Amount As Integer = 1) As Long
        Number = Number + Amount.SetMinValue(1)
        Return Number
    End Function

    ''' <summary>
    ''' Verifica se uma palavra é um Anagrama de outra palavra
    ''' </summary>
    ''' <param name="Text">       </param>
    ''' <param name="AnotherText"></param>
    ''' <returns></returns>
    <Extension()>
    Public Function IsAnagramOf(Text As String, AnotherText As String) As Boolean
        Dim char1 As Char() = Text.ToLower().ToCharArray()
        Dim char2 As Char() = Text.ToLower().ToCharArray()
        Array.Sort(char1)
        Array.Sort(char2)
        Dim NewWord1 As New String(char1)
        Dim NewWord2 As New String(char2)
        Return NewWord1 = NewWord2
    End Function

    ''' <summary>
    ''' Compara se uma string é igual a outras strings
    ''' </summary>
    ''' <param name="Text"> string principal</param>
    ''' <param name="Texts">strings para comparar</param>
    ''' <returns>TRUE se alguma das strings for igual a principal</returns>
    <Extension()>
    Public Function IsAny(Text As String, ParamArray Texts As String()) As Boolean
        For Each t As String In Texts
            If t = Text Then Return True
        Next
        Return False
    End Function

    ''' <summary>
    ''' Verifica se uma palavra ou frase é idêntica da direita para a esqueda bem como da esqueda
    ''' para direita
    ''' </summary>
    ''' <param name="Text">             Texto</param>
    ''' <param name="IgnoreWhiteSpaces">Ignora os espaços na hora de comparar</param>
    ''' <returns></returns>
    Public Function IsPalindrome(ByVal Text As String, Optional IgnoreWhiteSpaces As Boolean = False) As Boolean
        If IgnoreWhiteSpaces Then Text = Text.RemoveAny(" ")
        Dim c = Text.ToArray()
        Dim p = c
        Array.Reverse(p)
        Return p = c
    End Function

    ''' <summary>
    ''' Une todos os valores de um objeto em uma unica string
    ''' </summary>
    ''' <param name="Array">    Objeto com os valores</param>
    ''' <param name="Separator">Separador entre as strings</param>
    ''' <returns>string</returns>
    <Extension()>
    Public Function Join(Of Type)(Array As IEnumerable(Of Type), Optional Separator As String = ";") As String
        Return String.Join(Separator, Array)
    End Function

    ''' <summary>
    ''' Une todos os valores de um objeto em uma unica string
    ''' </summary>
    ''' <param name="Array">    Objeto com os valores</param>
    ''' <param name="Separator">Separador entre as strings</param>
    ''' <returns>string</returns>
    <Extension()>
    Public Function Join(Of Type)(Array As Type(), Optional Separator As String = ";") As String
        Return String.Join(Separator, Array)
    End Function

    ''' <summary>
    ''' Une todos os valores de um objeto em uma unica string
    ''' </summary>
    ''' <param name="Array">    Objeto com os valores</param>
    ''' <param name="Separator">Separador entre as strings</param>
    ''' <returns>string</returns>
    Public Function Join(Of Type)(Separator As String, ParamArray Array() As Type) As String
        Return String.Join(Separator, Array)
    End Function

    ''' <summary>
    ''' Une todos os valores de um objeto em uma unica string
    ''' </summary>
    ''' <param name="List">     Objeto com os valores</param>
    ''' <param name="Separator">Separador entre as strings</param>
    ''' <returns>string</returns>
    <Extension()>
    Public Function Join(Of Type)(List As List(Of Type), Optional Separator As String = "") As String
        Return List.ToArray.Join(Separator)
    End Function

    '''<summary>
    ''' Computa a distancia de Levenshtein entre 2 strings.
    ''' </summary>
    <Extension()> Public Function LevenshteinDistance(Text1 As String, Text2 As String) As Integer

        Dim n As Integer = Text1.Length
        Dim m As Integer = Text2.Length
        Dim d As Integer(,) = New Integer(n + 1, m + 1) {}

        ' Step 1
        If (n = 0) Then
            Return m
        End If

        If (m = 0) Then
            Return n
        End If

        ' Step 2
        For i As Integer = 0 To n
            d(i, 0) = i
        Next

        For j As Integer = 0 To m
            d(0, j) = j
        Next

        ' Step 3
        For i = 1 To n
            'Step 4
            For j = 1 To m
                ' Step 5
                Dim cost As Integer = If(Text2(j - 1) = Text1(i - 1), 0, 1)
                'Step 6
                d(i, j) = Math.Min(Math.Min(d(i - 1, j) + 1, d(i, j - 1) + 1), d(i - 1, j - 1) + cost)
            Next
        Next
        ' Step 7
        Return d(n, m)
    End Function

    ''' <summary>
    ''' limpa um texto deixando apenas os caracteres alfanumericos.
    ''' </summary>
    ''' <param name="Text"></param>
    ''' <returns></returns>
    <Extension> Function ParseAlphaNumeric(Text As String) As String
        Dim l As New List(Of String)
        For Each item In Text.Split(" ", StringSplitOptions.RemoveEmptyEntries)
            l.Add(Regex.Replace(item, "[^A-Za-z0-9]", ""))
        Next
        Return l.Join(" ")
    End Function

    ''' <summary>
    ''' Remove caracteres não numéricos de uma string
    ''' </summary>
    ''' <param name="Text">Texto</param>
    ''' <returns></returns>
    <Extension()> Function ParseDigits(ByVal Text As String, Optional Culture As CultureInfo = Nothing) As String
        Culture = If(Culture, CultureInfo.CurrentCulture)
        Dim strDigits As String = ""
        If Text = Nothing Then Return strDigits
        For Each c As Char In Text.ToCharArray()
            If Char.IsDigit(c) OrElse c = Convert.ToChar(Culture.NumberFormat.NumberDecimalSeparator) Then
                strDigits &= c
            End If
        Next c
        Return strDigits
    End Function

    <Extension()> Function ParseDigits(Of Type As IConvertible)(ByVal Text As String, Optional Culture As CultureInfo = Nothing) As Type
        Return CType(CType(Text.ParseDigits(Culture), Object), Type)
    End Function

    ''' <summary>
    ''' Transforma uma JSON String em um Objeto ou Classe
    ''' </summary>
    ''' <typeparam name="TypeClass">Objeto ou Classe</typeparam>
    ''' <param name="JSON">String JSON</param>
    ''' <returns>Um objeto do tipo T</returns>
    <Extension()> Public Function ParseJSON(Of TypeClass)(JSON As String, Optional DateFormat As String = "yyyy-MM-dd HH:mm:ss") As TypeClass
        Return New Json().Deserialize(Of TypeClass)(JSON)
    End Function

    ''' <summary>
    ''' Transforma uma JSON String em um Objeto ou Classe
    ''' </summary>
    ''' <param name="JSON">String JSON</param>
    ''' <returns>Um objeto do tipo T</returns>
    <Extension()> Public Function ParseJSON(JSON As String) As Object
        Return New Json().Deserialize(Of Object)(JSON)
    End Function

    ''' <summary>
    ''' Retorna uma string em sua forma poop
    ''' </summary>
    ''' <param name="Words"></param>
    ''' <returns></returns>
    Public Function Poopfy(ParamArray Words As String()) As String
        Dim p As New List(Of String)
        For Each Text In Words
            Dim l As Decimal = Text.Length / 2
            l = l.Floor
            If Not Text.GetFirstChars(l).Last.ToString.ToLower.IsIn({"a", "e", "i", "o", "u"}) Then
                l = l.ChangeType(Of Integer).Decrement
            End If
            p.Add(Text.GetFirstChars(l).Trim & Text.GetFirstChars(l).Reverse.ToList.Join.ToLower.Trim)
        Next
        Return p.Join(" ").Trim
    End Function

    ''' <summary>
    ''' Retorna uma string em sua forma poop
    ''' </summary>
    ''' <param name="Text"></param>
    ''' <returns></returns>
    <Extension()> Public Function Poopfy(Text As String) As String
        Return Poopfy(Text.Split(" "))
    End Function

    ''' <summary>
    ''' Return a Idented XML string
    ''' </summary>
    ''' <param name="Document"></param>
    ''' <returns></returns>
    <Extension()> Public Function PreetyPrint(Document As XmlDocument) As String
        Dim Result As [String] = ""

        Dim mStream As New MemoryStream()
        Dim writer As New XmlTextWriter(mStream, Encoding.Unicode)
        Try
            writer.Formatting = Formatting.Indented

            ' Write the XML into a formatting XmlTextWriter
            Document.WriteContentTo(writer)
            writer.Flush()
            mStream.Flush()

            ' Have to rewind the MemoryStream in order to read its contents.
            mStream.Position = 0

            ' Read MemoryStream contents into a StreamReader.
            Dim sReader As New StreamReader(mStream)

            ' Extract the text from the StreamReader.
            Return sReader.ReadToEnd()
        Catch generatedExceptionName As XmlException
        End Try

        mStream.Close()
        writer.Close()
        Return ""
    End Function

    ''' <summary>
    ''' Adiciona texto ao começo de uma string
    ''' </summary>
    ''' <param name="Text">       Texto</param>
    ''' <param name="PrependText">Texto adicional</param>
    <Extension()>
    Public Function Prepend(ByRef Text As String, PrependText As String) As String
        Text = PrependText & Text
        Return Text
    End Function

    ''' <summary>
    ''' Adiciona texto ao começo de uma string se um criterio for cumprido
    ''' </summary>
    ''' <param name="Text">       Texto</param>
    ''' <param name="PrependText">Texto adicional</param>
    ''' <param name="Test">       Teste</param>
    <Extension()> Public Function PrependIf(ByRef Text As String, PrependText As String, Test As Boolean) As String
        If Test Then
            Text.Prepend(PrependText)
        End If
        Return Text
    End Function

    ''' <summary>
    ''' Retorna o texto a na sua forma singular ou plural de acordo com um numero determinado em <paramref name="Identifier"/>.
    ''' </summary>
    ''' <param name="PluralText">Texto no plural</param>
    ''' <param name="Culture">   Cultura</param>
    ''' <param name="Identifier">Identificador da variavel quantificadora</param>
    ''' <returns></returns>
    ''' <example>texto = "total de {q=2 pães}"</example>
    <Extension()> Public Function QuantifyText(PluralText As String, Optional Culture As CultureInfo = Nothing, Optional Identifier As String = "q") As String
        Dim inst = PluralText.GetAllBetween("{" & Identifier & "=", "}")
        Dim no_wrap As Boolean = False
        If inst.Length = 0 Then
            no_wrap = True
            inst = {PluralText}
        End If
        For Each q In inst
            Dim numero = q.GetBefore(" ")
            If Not numero.IsNumber Then
                numero = numero.Length
            End If
            Dim texto = q.GetAfter(" ")
            Dim has_number As Boolean = texto.ContainsAny("[" & Identifier & "]")

            texto = texto.QuantifyText(numero, Culture)
            Dim newtxt = numero & " " & texto
            If has_number Then
                texto = texto.Replace("[" & Identifier & "]", numero)
                newtxt = texto
            End If
            If no_wrap Then
                PluralText = newtxt
            Else
                PluralText = PluralText.Replace("{" & Identifier & "=" + q + "}", newtxt)
            End If
        Next
        Return PluralText
    End Function

    ''' <summary>
    ''' Retorna o texto a na sua forma singular ou plural de acordo com um numero determinado.
    ''' </summary>
    ''' <param name="PluralText">Texto no plural</param>
    ''' <param name="Quantity">  Quantidade de Itens</param>
    ''' <param name="culture">   Cultura</param>
    ''' <returns></returns>
    <Extension()> Public Function QuantifyText(PluralText As String, Quantity As Object, Optional Culture As CultureInfo = Nothing) As String
        If Culture Is Nothing Then Culture = CultureInfo.CurrentCulture
        Dim nums As Integer() = {}
        Dim numero As Decimal = 0
        Select Case True
            Case GetType(String) Is Quantity.GetType AndAlso CType(Quantity, String).IsNumber
                numero = CType(Quantity, Decimal)
                Exit Select
            Case GetType(IList).IsAssignableFrom(Quantity.GetType)
                numero = CType(Quantity, IList).Count
                Exit Select
            Case GetType(IDictionary).IsAssignableFrom(Quantity.GetType)
                numero = CType(Quantity, IDictionary).Count
                Exit Select
            Case Else
                numero = CType(Quantity, Decimal)
        End Select

        If CultureInfo.GetCultureInfo("pt-BR").Equals(Culture) Then
            nums = {-1, 0, 1}
        Else
            nums = {-1, 1}
        End If

        If nums.Contains(numero) Then
            Return PluralText.Singularize()
        End If
        Return PluralText
    End Function

    ''' <summary>
    ''' Encapsula um tento entre 2 caracteres (normalmente parentesis, chaves, aspas ou colchetes)
    ''' </summary>
    ''' <param name="Text">     Texto</param>
    ''' <param name="QuoteChar">Caractere de encapsulamento</param>
    ''' <returns></returns>
    <Extension()>
    Function Quote(Text As String, Optional QuoteChar As Char = """"c) As String
        Return QuoteChar & Text & QuoteChar.ToString.GetOppositeWrapChar
    End Function

    ''' <summary>
    ''' Encapsula um tento entre 2 textos (normalmente parentesis, chaves, aspas ou colchetes) se uma
    ''' condiçao for cumprida
    ''' </summary>
    ''' <param name="Text">     Texto</param>
    ''' <param name="QuoteChar">Caractere de encapsulamento</param>
    ''' <returns></returns>
    <Extension()>
    Function QuoteIf(Text As String, Condition As Boolean, Optional QuoteChar As String = """") As String
        Return If(Condition, Text.Quote(QuoteChar), Text)
    End Function

    ''' <summary>
    ''' Sorteia um item da Matriz
    ''' </summary>
    ''' <typeparam name="Type">Tipo da Matriz</typeparam>
    ''' <param name="Array">Matriz</param>
    ''' <returns>Um valor do tipo especificado</returns>
    Public Function RandomItem(Of Type)(ParamArray Array As Type()) As Type
        Return Array.GetRandomItem
    End Function

    ''' <summary>
    ''' Escapa caracteres exclusivos de uma regex
    ''' </summary>
    ''' <param name="Text"></param>
    ''' <returns></returns>
    <Extension()>
    Public Function RegexEscape(Text As String) As String
        Dim chars = {"."c, "$"c, "^"c, "{"c, "["c, "("c, "|"c, ")"c, "*"c, "+"c, "?"c, "|"c}
        Dim newstring As String = ""
        For Each c In Text.ToArray
            If c.IsIn(chars) Then
                newstring.Append("\" & c)
            Else
                newstring.Append(c)
            End If
        Next
        Return newstring
    End Function

    ''' <summary>
    ''' Remove os acentos de uma string
    ''' </summary>
    ''' <param name="Text">Texto</param>
    ''' <returns>String sem os acentos</returns>
    <Extension>
    Public Function RemoveAccents(ByVal Text As String) As String
        Dim s As String = Text.Normalize(NormalizationForm.FormD)
        Dim sb As New StringBuilder()
        Dim k As Integer = 0
        While k < s.Length
            Dim uc As UnicodeCategory = CharUnicodeInfo.GetUnicodeCategory(s(k))
            If uc <> UnicodeCategory.NonSpacingMark Then
                sb.Append(s(k))
            End If
            k.Increment
        End While
        Return sb.ToString()
    End Function

    ''' <summary>
    ''' Remove várias strings de uma string
    ''' </summary>
    ''' <param name="Text">  Texto</param>
    ''' <param name="Values">Strings a serem removidas</param>
    ''' <returns>Uma string com os valores removidos</returns>
    <Extension>
    Public Function RemoveAny(ByVal Text As String, ParamArray Values() As String) As String
        Text = Text.ReplaceMany("", Values)
        Return Text
    End Function

    ''' <summary>
    ''' Remove os acentos de uma string
    ''' </summary>
    ''' <param name="Text">Texto</param>
    ''' <returns>String sem os acentos</returns>
    <Extension()>
    Public Function RemoveDiacritics(ByRef Text As String) As String
        Text = Text.RemoveAccents
        Return Text
    End Function

    ''' <summary>
    ''' Remove o final de uma string se ela for igual a qualquer um dos valores correspondentes
    ''' </summary>
    ''' <param name="Text">              Texto</param>
    ''' <param name="ContinuouslyRemove">
    ''' Parametro que indica se a string deve continuar sendo testada até que todas as ocorrencias
    ''' sejam removidas
    ''' </param>
    ''' <param name="StartStringTest">   Conjunto de textos que serão comparados</param>
    ''' <returns></returns>
    <Extension()>
    Public Function RemoveFirstAny(ByVal Text As String, ContinuouslyRemove As Boolean, ParamArray StartStringTest As String()) As String
        Dim re = Text
        While re.StartsWithAny(StartStringTest)
            For Each item In StartStringTest
                If re.StartsWith(item) Then
                    re = re.RemoveFirstIf(item)
                    If Not ContinuouslyRemove Then Return re
                End If
            Next
        End While
        Return re
    End Function

    ''' <summary>
    ''' Remove uma determinada linha de um texto
    ''' </summary>
    ''' <param name="Text">     Texto completo</param>
    ''' <param name="LineIndex">numero da linha a ser removida (Começando do 0)</param>
    ''' <returns>string sem a linha indicada</returns>
    ''' <summary>
    ''' Remove os X primeiros caracteres
    ''' </summary>
    ''' <param name="Text">     Texto</param>
    ''' <param name="Quantity"> Quantidade de Caracteres</param>
    ''' <returns></returns>
    <Extension()>
    Public Function RemoveFirstChars(Text As String, Optional Quantity As Integer = 1) As String
        If Text.Length > Quantity Then
            Return Text.Remove(0, Quantity)
        Else
            Return ""
        End If
    End Function

    ''' <summary>
    ''' Remove um texto do inicio de uma string se ele for um outro texto especificado
    ''' </summary>
    ''' <param name="Text">           Texto</param>
    ''' <param name="StartStringTest">Texto inicial que será comparado</param>
    <Extension>
    Function RemoveFirstIf(ByVal Text As String, StartStringTest As String) As String
        If Text.StartsWith(StartStringTest) Then
            Text = Text.RemoveFirstChars(StartStringTest.Length)
        End If
        Return Text
    End Function

    <Extension()>
    Public Function RemoveHTML(Text As String) As String
        If Text.IsNotBlank Then
            Return Regex.Replace(Text, "<.*?>", String.Empty).HtmlDecode
        End If
        Return ""
    End Function

    ''' <summary>
    ''' Remove o final de uma string se ela for igual a qualquer um dos valores correspondentes
    ''' </summary>
    ''' <param name="Text">              Texto</param>
    ''' <param name="ContinuouslyRemove">
    ''' Parametro que indica se a string deve continuar sendo testada até que todas as ocorrencias
    ''' sejam removidas
    ''' </param>
    ''' <param name="EndStringTest">     Conjunto de textos que serão comparados</param>
    ''' <returns></returns>
    <Extension()>
    Public Function RemoveLastAny(ByVal Text As String, ContinuouslyRemove As Boolean, ParamArray EndStringTest As String()) As String
        Dim re = Text
        While re.EndsWithAny(EndStringTest)
            For Each item In EndStringTest
                If re.EndsWith(item) Then
                    re = re.RemoveLastIf(item)
                    If Not ContinuouslyRemove Then Return re
                End If
            Next
        End While
        Return re
    End Function

    ''' <summary>
    ''' Remove os X ultimos caracteres
    ''' </summary>
    ''' <param name="Text">    Texto</param>
    ''' <param name="Quantity">Quantidade de Caracteres</param>
    ''' <returns></returns>
    <Extension()>
    Public Function RemoveLastChars(Text As String, Optional Quantity As Integer = 1) As String
        Return Text.Substring(0, Text.Length - Quantity)
    End Function

    ''' <summary>
    ''' Remove um texto do final de uma string se ele for um outro texto
    ''' </summary>
    ''' <param name="Text">         Texto</param>
    ''' <param name="EndStringTest">Texto final que será comparado</param>
    <Extension>
    Function RemoveLastIf(ByVal Text As String, EndStringTest As String) As String
        If Text.EndsWith(EndStringTest) Then
            Text = Text.RemoveLastChars(EndStringTest.Length)
        End If
        Return Text
    End Function

    ''' <summary>
    ''' Remove caracteres não printaveis de uma string
    ''' </summary>
    ''' <param name="Text">Texto</param>
    ''' <returns>String corrigida</returns>
    <Extension()>
    Public Function RemoveNonPrintable(Text As String) As String
        For Each c As Char In Text.ToCharArray()
            If Char.IsControl(c) Then
                Text = Text.ReplaceNone(c)
            End If
        Next
        Return Text.Trim()
    End Function

    ''' <summary>
    ''' Faz uma busca em todos os elementos do array e aplica um ReplaceFrom comum
    ''' </summary>
    ''' <param name="Strings">        Array de strings</param>
    ''' <param name="OldValue">       Valor antigo que será substituido</param>
    ''' <param name="NewValue">       Valor utilizado para substituir o valor antigo</param>
    ''' <param name="ReplaceIfEquals">
    ''' Se TRUE, realiza o replace se o valor no array for idêntico ao Valor antigo, se FALSE realiza
    ''' um ReplaceFrom em quaisquer valores antigos encontrados dentro do valor do array
    ''' </param>
    ''' <returns></returns>
    <Extension()>
    Public Function Replace(Strings As String(), OldValue As String, NewValue As String, Optional ReplaceIfEquals As Boolean = True) As String()
        Dim NewArray As String() = Strings
        For index = 0 To Strings.Length - 1
            If ReplaceIfEquals Then
                If NewArray(index) = OldValue Then
                    NewArray(index) = NewValue
                End If
            Else
                NewArray(index) = NewArray(index).Replace(OldValue, NewValue)
            End If
        Next
        Return NewArray
    End Function

    ''' <summary>
    ''' Faz uma busca em todos os elementos de uma lista e aplica um ReplaceFrom comum
    ''' </summary>
    ''' <param name="Strings">        Array de strings</param>
    ''' <param name="OldValue">       Valor antigo que será substituido</param>
    ''' <param name="NewValue">       Valor utilizado para substituir o valor antigo</param>
    ''' <param name="ReplaceIfEquals">
    ''' Se TRUE, realiza o replace se o valor no array for idêntico ao Valor antigo, se FALSE realiza
    ''' um ReplaceFrom em quaisquer valores antigos encontrados dentro do valor do array
    ''' </param>
    ''' <returns></returns>
    <Extension()>
    Public Function Replace(Strings As List(Of String), OldValue As String, NewValue As String, Optional ReplaceIfEquals As Boolean = True) As List(Of String)
        Return Strings.ToArray.Replace(OldValue, NewValue, ReplaceIfEquals).ToList()
    End Function

    ''' <summary>
    ''' Aplica varios replaces a um texto a partir de um <see cref="IDictionary"/>
    ''' </summary>
    <Extension> Public Function ReplaceFrom(ByVal Text As String, Dic As IDictionary(Of String, String)) As String
        If Dic IsNot Nothing AndAlso Text.IsNotBlank Then
            For Each p In Dic
                Text = Text.Replace(p.Key, p.Value)
            Next
        End If
        Return Text
    End Function

    ''' <summary>
    ''' Aplica um replace a um texto baseando-se em um <see cref="IDictionary"/>.
    ''' </summary>
    <Extension> Public Function ReplaceFrom(ByVal Text As String, Dic As IDictionary(Of String, String()), Optional Comparison As StringComparison = StringComparison.InvariantCultureIgnoreCase) As String
        If Dic IsNot Nothing AndAlso Text.IsNotBlank Then
            For Each p In Dic
                Text = Text.SensitiveReplace(p.Key, p.Value)
            Next
        End If
        Return Text
    End Function

    ''' <summary>
    ''' Aplica um replace a um texto baseando-se em um <see cref="IDictionary"/>.
    ''' </summary>
    <Extension> Public Function ReplaceFrom(ByVal Text As String, Dic As IDictionary(Of String(), String), Optional Comparison As StringComparison = StringComparison.InvariantCultureIgnoreCase) As String
        If Dic IsNot Nothing AndAlso Text.IsNotBlank Then
            For Each p In Dic
                Text = Text.SensitiveReplace(p.Value, p.Key.ToArray)
            Next
        End If
        Return Text
    End Function

    ''' <summary>
    ''' Aplica um replace a um texto baseando-se em um <see cref="IDictionary"/>.
    ''' </summary>
    <Extension> Public Function ReplaceFrom(ByVal Text As String, Dic As IDictionary(Of String(), String()), Optional Comparison As StringComparison = StringComparison.InvariantCultureIgnoreCase) As String
        If Dic IsNot Nothing AndAlso Text.IsNotBlank Then
            For Each p In Dic
                Dim froms = p.Key.ToList
                Dim tos = p.Value.ToList
                While froms.Count > tos.Count
                    tos.Add(String.Empty)
                End While
                For i = 0 To froms.Count - 1
                    Text = Text.SensitiveReplace(froms(i), tos(i))
                Next
            Next
        End If
        Return Text
    End Function

    ''' <summary>
    ''' Retorna uma nova sequência na qual todas as ocorrências de uma String especificada são
    ''' substituídas por um novo valor.
    ''' </summary>
    ''' <param name="Text">     Texto</param>
    ''' <param name="NewValue"> Novo Valor</param>
    ''' <param name="OldValues">Valores a serem substituido por um novo valor</param>
    ''' <returns></returns>
    <Extension>
    Public Function ReplaceMany(ByVal Text As String, NewValue As String, ParamArray OldValues As String()) As String
        For Each word In OldValues
            Text = Text.Replace(word, NewValue)
        Next
        Return Text
    End Function

    ''' <summary>
    ''' Retorna uma nova sequência na qual todas as ocorrências de uma String especificada são
    ''' substituídas por vazio.
    ''' </summary>
    ''' <param name="Text">    Texto</param>
    ''' <param name="OldValue">Valor a ser substituido por vazio</param>
    ''' <returns>String corrigida</returns>
    <Extension>
    Public Function ReplaceNone(Text As String, OldValue As String) As String
        Return Text.Replace(OldValue, "")
    End Function

    ''' <summary>
    ''' Realiza um replace em uma string usando um tipo especifico de comparacao
    ''' </summary>
    ''' <param name="Text">          </param>
    ''' <param name="NewValue">      </param>
    ''' <param name="OldValue">      </param>
    ''' <param name="ComparisonType"></param>
    ''' <returns></returns>
    <Extension()>
    Public Function SensitiveReplace(ByVal Text As String, ByVal NewValue As String, ByVal OldValue As String, Optional ComparisonType As StringComparison = StringComparison.InvariantCulture) As String
        Return SensitiveReplace(Text, NewValue, {OldValue}, ComparisonType)
    End Function

    ''' <summary>
    ''' Realiza um replace em uma string usando um tipo especifico de comparacao
    ''' </summary>
    ''' <param name="Text">          </param>
    ''' <param name="NewValue">      </param>
    ''' <param name="OldValues">     </param>
    ''' <param name="ComparisonType"></param>
    ''' <returns></returns>
    <Extension()>
    Public Function SensitiveReplace(ByVal Text As String, ByVal NewValue As String, ByVal OldValues As IEnumerable(Of String), Optional ComparisonType As StringComparison = StringComparison.InvariantCulture) As String
        If Text.IsNotBlank Then
            For Each oldvalue In If(OldValues, {""})
                NewValue = If(NewValue, String.Empty)
                If Not oldvalue.Equals(NewValue, ComparisonType) Then
                    Dim foundAt As Integer
                    Do
                        foundAt = Text.IndexOf(oldvalue, 0, ComparisonType)
                        If foundAt > -1 Then
                            Text = Text.Remove(foundAt, oldvalue.Length).Insert(foundAt, NewValue)
                        End If
                    Loop While foundAt <> -1
                End If
            Next
        End If
        Return Text
    End Function

    ''' <summary>
    ''' Transforma um Objeto em JSON utilizando o método ToJson() do objeto. Caso o método não existir, utiliza-se <see cref="Json.SerializeJSON(Object)"/>
    ''' </summary>
    ''' <param name="Obj">Objeto</param>
    ''' <returns>Uma String JSON</returns>
    <Extension()> Public Function SerializeJSON(Obj As Object, ParamArray params As Object()) As String
        Dim mds = Obj.GetType.GetMethods().Where(Function(x) x.ReturnType = GetType(String) AndAlso x.Name.ToLower.ContainsAny("tojson"))
        If mds.Count > 0 Then
            Return mds.First.Invoke(Obj, params)
        Else
            Return Json.SerializeJSON(Obj)
        End If
    End Function

    ''' <summary>
    ''' Randomiza a ordem dos itens de um Array
    ''' </summary>
    ''' <typeparam name="Type">Tipo do Array</typeparam>
    ''' <param name="Array">Matriz</param>
    <Extension()> Public Function Shuffle(Of Type)(ByRef Array As Type()) As Type()
        Array = Array.OrderByRandom().ToArray
        Return Array
    End Function

    ''' <summary>
    ''' Randomiza a ordem dos itens de uma Lista
    ''' </summary>
    ''' <typeparam name="Type">Tipo de Lista</typeparam>
    ''' <param name="List">Matriz</param>
    <Extension()> Public Function Shuffle(Of Type)(ByRef List As List(Of Type)) As List(Of Type)
        List = List.ToArray.Shuffle.ToList()
        Return List
    End Function

    ''' <summary>
    ''' Aleatoriza a ordem das letras de um texto
    ''' </summary>
    ''' <param name="Text">Texto</param>
    ''' <returns></returns>
    <Extension()> Public Function Shuffle(ByRef Text As String) As String
        Text = RandomWord(Text)
        Return Text
    End Function

    ''' <summary>
    ''' Retorna a frase especificada em sua forma singular
    ''' </summary>
    ''' <param name="Text">Texto no pluiral</param>
    ''' <returns></returns>
    <Extension()> Public Function Singularize(Text As String) As String
        Dim phrase As String() = Text.ApplySpaceOnWrapChars.Split(" ")
        For index = 0 To phrase.Count - 1

            Dim endchar As String = phrase(index).GetLastChars
            If endchar.IsAny(WordSplitters) Then
                phrase(index) = phrase(index).RemoveLastIf(endchar)
            End If

            Select Case True
                Case phrase(index).IsNumber OrElse phrase(index).IsEmail OrElse phrase(index).IsURL OrElse phrase(index).IsIP OrElse phrase(index).IsIn(WordSplitters)
                    'nao alterar estes tipos
                    Exit Select
                Case phrase(index).EndsWith("ões")
                    phrase(index) = phrase(index).RemoveLastIf("ões").Append("ão")
                    Exit Select
                Case phrase(index).EndsWith("ãos")
                    phrase(index) = phrase(index).RemoveLastIf("ãos").Append("ão")
                    Exit Select
                Case phrase(index).EndsWith("ães")
                    phrase(index) = phrase(index).RemoveLastIf("ães").Append("ão")
                    Exit Select
                Case phrase(index).EndsWith("es")
                    phrase(index) = phrase(index).RemoveLastIf("es")
                    Exit Select
                Case phrase(index).EndsWith("ns")
                    phrase(index) = phrase(index).RemoveLastIf("ns").Append("m")
                    Exit Select
                Case phrase(index).EndsWith("s")
                    phrase(index) = phrase(index).RemoveLastIf("s")
                    Exit Select
                Case Else
                    'ja esta no singular
            End Select
            If endchar.IsAny(WordSplitters) Then
                phrase(index).Append(endchar)
            End If
        Next
        Return phrase.Join(" ").AdjustWhiteSpaces
    End Function

    ''' <summary>
    ''' Corta un texto para exibir um numero máximo de caracteres ou na primeira quebra de linha.
    ''' </summary>
    ''' <param name="Text"></param>
    ''' <param name="TextLength"></param>
    ''' <param name="Ellipsis"></param>
    ''' <returns></returns>
    <Extension()>
    Public Function Slice(Text As String, Optional TextLength As Integer = 0, Optional Ellipsis As String = "...") As String
        If Text.Length <= TextLength OrElse TextLength < 1 Then
            Return Text
        Else
            Text = Text.GetBefore(Environment.NewLine)
            Return Text.GetFirstChars(TextLength) & Ellipsis
        End If
    End Function

    ''' <summary>
    ''' Separa um texto em um array de strings a partir de uma outra string
    ''' </summary>
    ''' <param name="Text">     Texto</param>
    ''' <param name="Separator">Texto utilizado como separador</param>
    ''' <returns></returns>
    <Extension>
    Public Function Split(Text As String, Separator As String, Optional Options As StringSplitOptions = StringSplitOptions.RemoveEmptyEntries) As String()
        Return If(Text, "").Split({Separator}, Options)
    End Function

    ''' <summary>
    ''' Verifica se uma string começa com alguma outra string de um array
    ''' </summary>
    ''' <param name="Text"> </param>
    ''' <param name="Words"></param>
    ''' <returns></returns>
    <Extension()> Function StartsWithAny(Text As String, ParamArray Words As String()) As Boolean
        Return Words.Any(Function(p) Text.StartsWith(p))
    End Function

    ''' <summary>
    ''' Conta as silabas de uma palavra
    ''' </summary>
    ''' <param name="Word"></param>
    ''' <returns></returns>
    <Extension()> Function SyllableCount(ByVal Word As String) As Integer
        Word = Word.ToLower().Trim()
        Dim lastWasVowel As Boolean
        Dim vowels = {"a"c, "e"c, "i"c, "o"c, "u"c, "y"c}.ToList()
        Dim count As Integer
        For Each c In Word
            If vowels.Contains(c) Then
                If Not lastWasVowel Then count += 1
                lastWasVowel = True
            Else
                lastWasVowel = False
            End If
        Next
        If (Word.EndsWith("e") OrElse (Word.EndsWith("es") OrElse Word.EndsWith("ed"))) AndAlso Not Word.EndsWith("le") Then count -= 1
        Return count
    End Function

    ''' <summary>
    ''' Alterna maiusculas e minusculas para cada letra de uma string
    ''' </summary>
    ''' <param name="Text"></param>
    ''' <returns></returns>
    <Extension> Public Function ToAlternateCase(Text As String) As String
        Dim ch = Text.ToArray
        For index = 0 To ch.Length - 1
            Dim antec = ch.IfNoIndex(index - 1, "")

            If antec.ToString.IsBlank OrElse Char.IsLower(antec) OrElse antec = vbNullChar Then
                ch(index) = Char.ToUpper(ch(index))
            Else
                ch(index) = Char.ToLower(ch(index))
            End If
        Next
        Return New String(ch)
    End Function

    ''' <summary>
    ''' Retorna um anagrama de um texto
    ''' </summary>
    ''' <param name="Text">Texto</param>
    ''' <returns></returns>
    <Extension()> Public Function ToAnagram(ByVal Text As String) As String
        Return Text.Shuffle().AdjustWhiteSpaces
    End Function

    ''' <summary>
    ''' Retorna uma lista com todos os anagramas de uma palavra (Metodo Lento)
    ''' </summary>
    ''' <param name="Text">Texto</param>
    ''' <returns>Lista de anagramas</returns>
    <Extension()> Public Function ToAnagramList(ByVal Text As String) As List(Of String)
        ToAnagramList = New List(Of String) From {Text}
        Dim i As Int32
        Dim y As Int32
        Dim x As Int32
        Dim tempChar As String
        Dim newString As String
        Dim strings(,) As String
        Dim rowCount As Long

        If Text.Length < 2 Then
            Exit Function
        End If

        'use the factorial function to determine the number of rows needed
        'because redim preserve is slow
        ReDim strings(Text.Length - 1, Factorial(Text.Length - 1) - 1)
        strings(0, 0) = Text

        'swap each character(I) from the second postion to the second to last position
        For i = 1 To (Text.Length - 2)
            'for each of the already created numbers
            For y = 0 To rowCount
                'do swaps for the character(I) with each of the characters to the right
                For x = Text.Length To i + 2 Step -1
                    tempChar = strings(0, y).Substring(i, 1)
                    newString = strings(0, y)
                    Mid(newString, i + 1, 1) = newString.Substring(x - 1, 1)
                    Mid(newString, x, 1) = tempChar
                    rowCount = rowCount + 1
                    strings(0, rowCount) = newString
                Next
            Next
        Next

        'Shift Characters
        'for each empty column
        For i = 1 To Text.Length - 1
            'move the shift character over one
            For x = 0 To strings.GetUpperBound(1)
                strings(i, x) = strings(i - 1, x)
                Mid(strings(i, x), i, 1) = strings(i - 1, x).Substring(i, 1)
                Mid(strings(i, x), i + 1, 1) = strings(i - 1, x).Substring(i - 1, 1)
            Next
        Next

        Dim jagged As String()() = New String(strings.GetLength(0) - 1)() {}
        For ii As Integer = 0 To strings.GetLength(0) - 1
            jagged(ii) = New String(strings.GetLength(1) - 1) {}
            For j As Integer = 0 To strings.GetLength(1) - 1
                jagged(ii)(j) = strings(ii, j)
            Next
        Next
        For Each item In jagged
            ToAnagramList.AddRange(item)
        Next
        Return ToAnagramList.Distinct().OrderBy(Function(o) o).ToList()
    End Function

    ''' <summary>
    ''' Transforma uma frase em uma palavra CamelCase
    ''' </summary>
    ''' <param name="Text">Texto a ser manipulado</param>
    ''' <returns>Uma String com o texto am CameCase</returns>
    <Extension()>
    Public Function ToCamel(Text As String) As String
        Return ToProper(Text).Split(" ", StringSplitOptions.RemoveEmptyEntries).Join("")
    End Function

    ''' <summary>
    ''' Transforma um numero em sua forma extensa (com até 3 casas apos a virgula)
    ''' </summary>
    ''' <param name="Number">       Numero decimal</param>
    ''' <param name="DecimalPlaces">Numero de casas decimais (de 0 a 3)</param>
    ''' <returns>String contendo o numero por extenso</returns>
    <Extension()> Public Function ToExtensiveForm(ByVal Number As Decimal, Optional DecimalPlaces As Integer = 3) As String
        Dim dec As Long = Number.GetDecimalPlaces(DecimalPlaces.LimitRange(0, 3))
        Dim num As Long = Number.Floor
        Return (num.InExtensive & If(dec = 0 Or DecimalPlaces = 0, "", " vírgula " & dec.InExtensive)).ToLower.AdjustWhiteSpaces
    End Function

    ''' <summary>
    ''' Transforma um numero em sua forma extensa
    ''' </summary>
    ''' <param name="Number">Numero decimal</param>
    ''' <returns>String contendo o numero por extenso</returns>
    <Extension()> Public Function ToExtensiveForm(ByVal Number As Integer) As String
        Return ToExtensiveForm(Number.ChangeType(Of Decimal), 0)
    End Function

    ''' <summary>
    ''' Transforma um valor monetário em sua forma extensa
    ''' </summary>
    ''' <param name="MoneyValue">Numero decimal</param>
    ''' <returns>String contendo o numero por extenso</returns>
    <Extension()> Public Function ToExtensiveForm(ByVal MoneyValue As Money) As String
        Return MoneyValue.Value.ToExtensiveMoneyForm
    End Function

    ''' <summary>
    ''' Transforma um valor monetário R$ em sua forma extensa
    ''' </summary>
    ''' <param name="Value">Numero decimal</param>
    ''' <returns>String contendo o numero por extenso</returns>
    <Extension()> Public Function ToExtensiveMoneyForm(ByVal Value As Decimal) As String
        Dim decimalplaces As Long = Value.GetDecimalPlaces(2)
        Dim num As Long = Value.Floor
        Return (num.ChangeType(Of Long).InExtensive & " Reais " & If(decimalplaces = 0, "", " e " & decimalplaces.InExtensive & " Centavos")).ToLower().AdjustWhiteSpaces
    End Function

    ''' <summary>
    ''' Retorna o uma string representando um valor em bytes, KB, MB ou TB
    ''' </summary>
    ''' <param name="Size">Tamanho</param>
    ''' <returns>String com o tamanho + unidade de medida</returns>
    <Extension()>
    Public Function ToFileSizeString(ByVal Size As Byte()) As String
        Return Size.LongLength.ToFileSizeString
    End Function

    ''' <summary>
    ''' Retorna o uma string representando um valor em bytes, KB, MB ou TB
    ''' </summary>
    ''' <param name="Size">Tamanho</param>
    ''' <returns>String com o tamanho + unidade de medida</returns>
    <Extension()>
    Public Function ToFileSizeString(ByVal Size As FileInfo) As String
        Return Size.Length.ToFileSizeString
    End Function

    ''' <summary>
    ''' Retorna o uma string representando um valor em bytes, KB, MB ou TB
    ''' </summary>
    ''' <param name="Size">Tamanho</param>
    ''' <returns>String com o tamanho + unidade de medida</returns>
    <Extension()>
    Public Function ToFileSizeString(ByVal Size As Double) As String
        Return Size.ChangeType(Of Decimal).ToFileSizeString
    End Function

    ''' <summary>
    ''' Retorna o uma string representando um valor em bytes, KB, MB ou TB
    ''' </summary>
    ''' <param name="Size">Tamanho</param>
    ''' <returns>String com o tamanho + unidade de medida</returns>
    <Extension()>
    Public Function ToFileSizeString(ByVal Size As Integer) As String
        Return Size.ChangeType(Of Decimal).ToFileSizeString
    End Function

    ''' <summary>
    ''' Retorna o uma string representando um valor em bytes, KB, MB ou TB
    ''' </summary>
    ''' <param name="Size">Tamanho</param>
    ''' <returns>String com o tamanho + unidade de medida</returns>
    <Extension()>
    Public Function ToFileSizeString(ByVal Size As Long) As String
        Return Size.ChangeType(Of Decimal).ToFileSizeString
    End Function

    ''' <summary>
    ''' Retorna o uma string representando um valor em bytes, KB, MB ou TB
    ''' </summary>
    ''' <param name="Size">Tamanho</param>
    ''' <returns>String com o tamanho + unidade de medida</returns>
    <Extension()>
    Public Function ToFileSizeString(ByVal Size As Decimal) As String
        Return UnitConverter.CreateFileSizeConverter.Abreviate(Size)
    End Function

    ''' <summary>
    ''' Prepara uma string para se tornar uma URL amigavel (remove caracteres nao permitidos e troca
    ''' espacos por hifen)
    ''' </summary>
    ''' <param name="Text">         </param>
    ''' <param name="UseUnderscore">
    ''' Indica se os espacos serão substituidos por underscores (underline). Use FALSE para hifens
    ''' </param>
    ''' <returns>string amigavel para URL</returns>
    <Extension()>
    Public Function ToFriendlyURL(Text As String, Optional UseUnderscore As Boolean = False) As String
        Return Text.Replace(" ", If(UseUnderscore, "_", "-")).Replace("&", "e").Replace("@", "a").RemoveAny(".", ",", "?", "/", "#", "\", "<", ">", "(", ")", "{", "}", "[", "]", "|", """", "'", vbTab, Environment.NewLine).Trim.RemoveAccents().ToLower()
    End Function

    ''' <summary>
    ''' Transforma uma lista em uma lista HTML (OL ou UL)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="List">       </param>
    ''' <param name="OrdenedList"></param>
    ''' <returns></returns>
    <Extension> Public Function ToHtmlList(Of T)(List As IEnumerable(Of T), Optional OrdenedList As Boolean = False) As HtmlParser.HtmlElement
        Return List.Select(Function(x) x.ToString.WrapInTag("li").ToString).Join("").WrapInTag(If(OrdenedList, "ol", "ul"))
    End Function

    ''' <summary>
    ''' Transforma um HtmlGenericControl em uma stringHTML
    ''' </summary>
    ''' <param name="Control">Controle HTML</param>
    ''' <returns></returns>
    <Extension>
    Public Function ToHtmlString(Control As HtmlGenericControl) As String
        Dim c As New HtmlParser.HtmlElement(Control.TagName, Control.InnerHtml)
        For Each k In Control.Attributes.Keys
            If Not k = "innerhtml" Then
                c.Attributes.Add(k, Control.Attributes(k))
            End If
        Next
        Return c.ToString
    End Function

    ''' <summary>
    ''' Converte um texo para Leet (1337)
    ''' </summary>
    ''' <param name="text">  TExto original</param>
    ''' <param name="degree">Grau de itensidade (0 - 100%)</param>
    ''' <returns>Texto em 1337</returns>
    <Extension>
    Public Function ToLeet(Text As String, Optional Degree As Integer = 30) As String
        ' Adjust degree between 0 - 100
        Degree.LimitRange(0, 100)
        ' No Leet Translator
        If Degree = 0 Then
            Return Text
        End If
        ' StringBuilder to store result.
        Dim sb As New StringBuilder(Text.Length)
        For Each c As Char In Text
            '#Region "Degree > 0 and < 17"
            If Degree < 17 AndAlso Degree > 0 Then
                Select Case c
                    Case "e"c
                        sb.Append("3")
                        Exit Select
                    Case "E"c
                        sb.Append("3")
                        Exit Select
                    Case Else
                        sb.Append(c)
                        Exit Select
                End Select
                '#End Region
                '#Region "Degree > 16 and < 33"
            ElseIf Degree < 33 AndAlso Degree > 16 Then
                Select Case c
                    Case "a"c
                        sb.Append("4")
                        Exit Select
                    Case "e"c
                        sb.Append("3")
                        Exit Select
                    Case "i"c
                        sb.Append("1")
                        Exit Select
                    Case "o"c
                        sb.Append("0")
                        Exit Select
                    Case "A"c
                        sb.Append("4")
                        Exit Select
                    Case "E"c
                        sb.Append("3")
                        Exit Select
                    Case "I"c
                        sb.Append("1")
                        Exit Select
                    Case "O"c
                        sb.Append("0")
                        Exit Select
                    Case Else
                        sb.Append(c)
                        Exit Select
                End Select
                '#End Region
                '#Region "Degree > 32 and < 49"
            ElseIf Degree < 49 AndAlso Degree > 32 Then
                Select Case c
                    Case "a"c
                        sb.Append("4")
                        Exit Select
                    Case "e"c
                        sb.Append("3")
                        Exit Select
                    Case "i"c
                        sb.Append("1")
                        Exit Select
                    Case "o"c
                        sb.Append("0")
                        Exit Select
                    Case "A"c
                        sb.Append("4")
                        Exit Select
                    Case "E"c
                        sb.Append("3")
                        Exit Select
                    Case "I"c
                        sb.Append("1")
                        Exit Select
                    Case "O"c
                        sb.Append("0")
                        Exit Select
                    Case "s"c
                        sb.Append("$")
                        Exit Select
                    Case "S"c
                        sb.Append("$")
                        Exit Select
                    Case "l"c
                        sb.Append("£")
                        Exit Select
                    Case "L"c
                        sb.Append("£")
                        Exit Select
                    Case "c"c
                        sb.Append("(")
                        Exit Select
                    Case "C"c
                        sb.Append("(")
                        Exit Select
                    Case "y"c
                        sb.Append("¥")
                        Exit Select
                    Case "Y"c
                        sb.Append("¥")
                        Exit Select
                    Case "U"c
                        sb.Append("µ")
                        Exit Select
                    Case "u"c
                        sb.Append("µ")
                        Exit Select
                    Case "d"c
                        sb.Append("Ð")
                        Exit Select
                    Case "D"c
                        sb.Append("Ð")
                        Exit Select
                    Case Else
                        sb.Append(c)
                        Exit Select
                End Select
                '#End Region
                '#Region "Degree > 48 and < 65"
            ElseIf Degree < 65 AndAlso Degree > 48 Then
                Select Case c
                    Case "a"c
                        sb.Append("4")
                        Exit Select
                    Case "e"c
                        sb.Append("3")
                        Exit Select
                    Case "i"c
                        sb.Append("1")
                        Exit Select
                    Case "o"c
                        sb.Append("0")
                        Exit Select
                    Case "A"c
                        sb.Append("4")
                        Exit Select
                    Case "E"c
                        sb.Append("3")
                        Exit Select
                    Case "I"c
                        sb.Append("1")
                        Exit Select
                    Case "O"c
                        sb.Append("0")
                        Exit Select
                    Case "k"c
                        sb.Append("|{")
                        Exit Select
                    Case "K"c
                        sb.Append("|{")
                        Exit Select
                    Case "s"c
                        sb.Append("$")
                        Exit Select
                    Case "S"c
                        sb.Append("$")
                        Exit Select
                    Case "g"c
                        sb.Append("9")
                        Exit Select
                    Case "G"c
                        sb.Append("9")
                        Exit Select
                    Case "l"c
                        sb.Append("£")
                        Exit Select
                    Case "L"c
                        sb.Append("£")
                        Exit Select
                    Case "c"c
                        sb.Append("(")
                        Exit Select
                    Case "C"c
                        sb.Append("(")
                        Exit Select
                    Case "t"c
                        sb.Append("7")
                        Exit Select
                    Case "T"c
                        sb.Append("7")
                        Exit Select
                    Case "z"c
                        sb.Append("2")
                        Exit Select
                    Case "Z"c
                        sb.Append("2")
                        Exit Select
                    Case "y"c
                        sb.Append("¥")
                        Exit Select
                    Case "Y"c
                        sb.Append("¥")
                        Exit Select
                    Case "U"c
                        sb.Append("µ")
                        Exit Select
                    Case "u"c
                        sb.Append("µ")
                        Exit Select
                    Case "f"c
                        sb.Append("ƒ")
                        Exit Select
                    Case "F"c
                        sb.Append("ƒ")
                        Exit Select
                    Case "d"c
                        sb.Append("Ð")
                        Exit Select
                    Case "D"c
                        sb.Append("Ð")
                        Exit Select
                    Case Else
                        sb.Append(c)
                        Exit Select
                End Select
                '#End Region
                '#Region "Degree > 64 and < 81"
            ElseIf Degree < 81 AndAlso Degree > 64 Then
                Select Case c
                    Case "a"c
                        sb.Append("4")
                        Exit Select
                    Case "e"c
                        sb.Append("3")
                        Exit Select
                    Case "i"c
                        sb.Append("1")
                        Exit Select
                    Case "o"c
                        sb.Append("0")
                        Exit Select
                    Case "A"c
                        sb.Append("4")
                        Exit Select
                    Case "E"c
                        sb.Append("3")
                        Exit Select
                    Case "I"c
                        sb.Append("1")
                        Exit Select
                    Case "O"c
                        sb.Append("0")
                        Exit Select
                    Case "k"c
                        sb.Append("|{")
                        Exit Select
                    Case "K"c
                        sb.Append("|{")
                        Exit Select
                    Case "s"c
                        sb.Append("$")
                        Exit Select
                    Case "S"c
                        sb.Append("$")
                        Exit Select
                    Case "g"c
                        sb.Append("9")
                        Exit Select
                    Case "G"c
                        sb.Append("6")
                        Exit Select
                    Case "l"c
                        sb.Append("£")
                        Exit Select
                    Case "L"c
                        sb.Append("£")
                        Exit Select
                    Case "c"c
                        sb.Append("(")
                        Exit Select
                    Case "C"c
                        sb.Append("(")
                        Exit Select
                    Case "t"c
                        sb.Append("7")
                        Exit Select
                    Case "T"c
                        sb.Append("7")
                        Exit Select
                    Case "z"c
                        sb.Append("2")
                        Exit Select
                    Case "Z"c
                        sb.Append("2")
                        Exit Select
                    Case "y"c
                        sb.Append("¥")
                        Exit Select
                    Case "Y"c
                        sb.Append("¥")
                        Exit Select
                    Case "U"c
                        sb.Append("µ")
                        Exit Select
                    Case "u"c
                        sb.Append("µ")
                        Exit Select
                    Case "f"c
                        sb.Append("ƒ")
                        Exit Select
                    Case "F"c
                        sb.Append("ƒ")
                        Exit Select
                    Case "d"c
                        sb.Append("Ð")
                        Exit Select
                    Case "D"c
                        sb.Append("Ð")
                        Exit Select
                    Case "n"c
                        sb.Append("|\|")
                        Exit Select
                    Case "N"c
                        sb.Append("|\|")
                        Exit Select
                    Case "w"c
                        sb.Append("\/\/")
                        Exit Select
                    Case "W"c
                        sb.Append("\/\/")
                        Exit Select
                    Case "h"c
                        sb.Append("|-|")
                        Exit Select
                    Case "H"c
                        sb.Append("|-|")
                        Exit Select
                    Case "v"c
                        sb.Append("\/")
                        Exit Select
                    Case "V"c
                        sb.Append("\/")
                        Exit Select
                    Case "m"c
                        sb.Append("|\/|")
                        Exit Select
                    Case "M"c
                        sb.Append("|\/|")
                        Exit Select
                    Case Else
                        sb.Append(c)
                        Exit Select
                End Select
                '#End Region
                '#Region "Degree < 100 and > 80"
            ElseIf Degree > 80 AndAlso Degree < 100 Then
                Select Case c
                    Case "a"c
                        sb.Append("4")
                        Exit Select
                    Case "e"c
                        sb.Append("3")
                        Exit Select
                    Case "i"c
                        sb.Append("1")
                        Exit Select
                    Case "o"c
                        sb.Append("0")
                        Exit Select
                    Case "A"c
                        sb.Append("4")
                        Exit Select
                    Case "E"c
                        sb.Append("3")
                        Exit Select
                    Case "I"c
                        sb.Append("1")
                        Exit Select
                    Case "O"c
                        sb.Append("0")
                        Exit Select
                    Case "s"c
                        sb.Append("$")
                        Exit Select
                    Case "S"c
                        sb.Append("$")
                        Exit Select
                    Case "g"c
                        sb.Append("9")
                        Exit Select
                    Case "G"c
                        sb.Append("6")
                        Exit Select
                    Case "l"c
                        sb.Append("£")
                        Exit Select
                    Case "L"c
                        sb.Append("£")
                        Exit Select
                    Case "c"c
                        sb.Append("(")
                        Exit Select
                    Case "C"c
                        sb.Append("(")
                        Exit Select
                    Case "t"c
                        sb.Append("7")
                        Exit Select
                    Case "T"c
                        sb.Append("7")
                        Exit Select
                    Case "z"c
                        sb.Append("2")
                        Exit Select
                    Case "Z"c
                        sb.Append("2")
                        Exit Select
                    Case "y"c
                        sb.Append("¥")
                        Exit Select
                    Case "Y"c
                        sb.Append("¥")
                        Exit Select
                    Case "U"c
                        sb.Append("µ")
                        Exit Select
                    Case "u"c
                        sb.Append("µ")
                        Exit Select
                    Case "f"c
                        sb.Append("ƒ")
                        Exit Select
                    Case "F"c
                        sb.Append("ƒ")
                        Exit Select
                    Case "d"c
                        sb.Append("Ð")
                        Exit Select
                    Case "D"c
                        sb.Append("Ð")
                        Exit Select
                    Case "n"c
                        sb.Append("|\|")
                        Exit Select
                    Case "N"c
                        sb.Append("|\|")
                        Exit Select
                    Case "w"c
                        sb.Append("\/\/")
                        Exit Select
                    Case "W"c
                        sb.Append("\/\/")
                        Exit Select
                    Case "h"c
                        sb.Append("|-|")
                        Exit Select
                    Case "H"c
                        sb.Append("|-|")
                        Exit Select
                    Case "v"c
                        sb.Append("\/")
                        Exit Select
                    Case "V"c
                        sb.Append("\/")
                        Exit Select
                    Case "k"c
                        sb.Append("|{")
                        Exit Select
                    Case "K"c
                        sb.Append("|{")
                        Exit Select
                    Case "r"c
                        sb.Append("®")
                        Exit Select
                    Case "R"c
                        sb.Append("®")
                        Exit Select
                    Case "m"c
                        sb.Append("|\/|")
                        Exit Select
                    Case "M"c
                        sb.Append("|\/|")
                        Exit Select
                    Case "b"c
                        sb.Append("ß")
                        Exit Select
                    Case "B"c
                        sb.Append("ß")
                        Exit Select
                    Case "q"c
                        sb.Append("Q")
                        Exit Select
                    Case "Q"c
                        sb.Append("Q¸")
                        Exit Select
                    Case "x"c
                        sb.Append(")(")
                        Exit Select
                    Case "X"c
                        sb.Append(")(")
                        Exit Select
                    Case Else
                        sb.Append(c)
                        Exit Select
                End Select
                '#End Region
                '#Region "Degree 100"
            ElseIf Degree > 99 Then
                Select Case c
                    Case "a"c
                        sb.Append("4")
                        Exit Select
                    Case "e"c
                        sb.Append("3")
                        Exit Select
                    Case "i"c
                        sb.Append("1")
                        Exit Select
                    Case "o"c
                        sb.Append("0")
                        Exit Select
                    Case "A"c
                        sb.Append("4")
                        Exit Select
                    Case "E"c
                        sb.Append("3")
                        Exit Select
                    Case "I"c
                        sb.Append("1")
                        Exit Select
                    Case "O"c
                        sb.Append("0")
                        Exit Select
                    Case "s"c
                        sb.Append("$")
                        Exit Select
                    Case "S"c
                        sb.Append("$")
                        Exit Select
                    Case "g"c
                        sb.Append("9")
                        Exit Select
                    Case "G"c
                        sb.Append("6")
                        Exit Select
                    Case "l"c
                        sb.Append("£")
                        Exit Select
                    Case "L"c
                        sb.Append("£")
                        Exit Select
                    Case "c"c
                        sb.Append("(")
                        Exit Select
                    Case "C"c
                        sb.Append("(")
                        Exit Select
                    Case "t"c
                        sb.Append("7")
                        Exit Select
                    Case "T"c
                        sb.Append("7")
                        Exit Select
                    Case "z"c
                        sb.Append("2")
                        Exit Select
                    Case "Z"c
                        sb.Append("2")
                        Exit Select
                    Case "y"c
                        sb.Append("¥")
                        Exit Select
                    Case "Y"c
                        sb.Append("¥")
                        Exit Select
                    Case "U"c
                        sb.Append("µ")
                        Exit Select
                    Case "u"c
                        sb.Append("µ")
                        Exit Select
                    Case "f"c
                        sb.Append("ƒ")
                        Exit Select
                    Case "F"c
                        sb.Append("ƒ")
                        Exit Select
                    Case "d"c
                        sb.Append("Ð")
                        Exit Select
                    Case "D"c
                        sb.Append("Ð")
                        Exit Select
                    Case "n"c
                        sb.Append("|\|")
                        Exit Select
                    Case "N"c
                        sb.Append("|\|")
                        Exit Select
                    Case "w"c
                        sb.Append("\/\/")
                        Exit Select
                    Case "W"c
                        sb.Append("\/\/")
                        Exit Select
                    Case "h"c
                        sb.Append("|-|")
                        Exit Select
                    Case "H"c
                        sb.Append("|-|")
                        Exit Select
                    Case "v"c
                        sb.Append("\/")
                        Exit Select
                    Case "V"c
                        sb.Append("\/")
                        Exit Select
                    Case "k"c
                        sb.Append("|{")
                        Exit Select
                    Case "K"c
                        sb.Append("|{")
                        Exit Select
                    Case "r"c
                        sb.Append("®")
                        Exit Select
                    Case "R"c
                        sb.Append("®")
                        Exit Select
                    Case "m"c
                        sb.Append("|\/|")
                        Exit Select
                    Case "M"c
                        sb.Append("|\/|")
                        Exit Select
                    Case "b"c
                        sb.Append("ß")
                        Exit Select
                    Case "B"c
                        sb.Append("ß")
                        Exit Select
                    Case "j"c
                        sb.Append("_|")
                        Exit Select
                    Case "J"c
                        sb.Append("_|")
                        Exit Select
                    Case "P"c
                        sb.Append("|°")
                        Exit Select
                    Case "q"c
                        sb.Append("¶")
                        Exit Select
                    Case "Q"c
                        sb.Append("¶¸")
                        Exit Select
                    Case "x"c
                        sb.Append(")(")
                        Exit Select
                    Case "X"c
                        sb.Append(")(")
                        Exit Select
                    Case Else
                        sb.Append(c)
                        Exit Select
                End Select
                '#End Region
            End If
        Next
        Return sb.ToString()
        ' Return result.
    End Function

    ''' <summary>
    ''' Retorna um numero com o sinal de porcentagem
    ''' </summary>
    ''' <param name="Number"></param>
    ''' <returns></returns>
    <Extension()> Public Function ToPercentString(Number As Decimal, Optional Decimals As Integer = -1) As String
        If Decimals > -1 Then
            Number = Decimal.Round(Number, Decimals)
        End If
        Return Number.ToString.Append("%")
    End Function

    ''' <summary>
    ''' Retorna um numero com o sinal de porcentagem
    ''' </summary>
    ''' <param name="Number"></param>
    ''' <returns></returns>
    <Extension()> Public Function ToPercentString(Number As Integer) As String
        Return Number.ToString.Append("%")
    End Function

    ''' <summary>
    ''' Retorna um numero com o sinal de porcentagem
    ''' </summary>
    ''' <param name="Number"></param>
    ''' <returns></returns>
    <Extension()> Public Function ToPercentString(Number As Double, Optional Decimals As Integer = -1) As String
        If Decimals > -1 Then
            Number = Decimal.Round(Number.ChangeType(Of Decimal), Decimals)
        End If
        Return Number.ToString.Append("%")
    End Function

    ''' <summary>
    ''' Retorna um numero com o sinal de porcentagem
    ''' </summary>
    ''' <param name="Number"></param>
    ''' <returns></returns>
    <Extension()> Public Function ToPercentString(Number As Short) As String
        Return Number.ToString.Append("%")
    End Function

    ''' <summary>
    ''' Retorna um numero com o sinal de porcentagem
    ''' </summary>
    ''' <param name="Number"></param>
    ''' <returns></returns>
    <Extension()> Public Function ToPercentString(Number As Long) As String
        Return Number.ToString.Append("%")
    End Function

    ''' <summary>
    ''' Coloca o texto em TitleCase
    ''' </summary>
    ''' <param name="Text"></param>
    ''' <returns></returns>
    <Extension()>
    Public Function ToProper(Text As String) As String
        Return StrConv(Text, vbProperCase)
    End Function



    ''' <summary>
    ''' Coloca a string em Randomcase (aleatoriamente letras maiusculas ou minusculas)
    ''' </summary>
    ''' <param name="Text"></param>
    ''' <returns></returns>
    <Extension> Public Function ToRandomCase(Text As String) As String
        Dim ch = Text.ToArray
        Dim times = ch.Length
        For index = 1 To times
            Dim newindex = RandomNumber(0, ch.Length - 1)
            ch(newindex) = Char.ToUpper(ch(newindex))
        Next
        Return New String(ch)
    End Function

    ''' <summary>
    ''' Prepara uma string para se tornar uma URL amigavel (remove caracteres nao permitidos e troca
    ''' espacos por hifen). É um alias para <see cref="ToFriendlyURL(String, Boolean)"/>
    ''' </summary>
    ''' <param name="Text">         </param>
    ''' <param name="UseUnderscore">
    ''' Indica se os espacos serão substituidos por underscores (underline). Use FALSE para hifens
    ''' </param>
    ''' <returns>string amigavel para URL</returns>
    <Extension()> Function ToSlug(Text As String, Optional UseUnderscore As Boolean = False) As String
        Return Text.ToFriendlyURL(UseUnderscore)
    End Function

    ''' <summary>
    ''' Retorna uma string em Snake Case
    ''' </summary>
    ''' <param name="Text"></param>
    ''' <returns></returns>
    <Extension()>
    Public Function ToSnake(Text As String) As String
        Return Text.Replace(" ", "_")
    End Function

    ''' <summary>
    ''' Cria um <see cref="Stream"/> a partir de uma string
    ''' </summary>
    ''' <param name="TExt"></param>
    ''' <returns></returns>
    <Extension> Public Function ToStream(Text As String) As Stream
        Dim stream As New MemoryStream()
        Dim writer As New StreamWriter(stream)
        writer.Write(Text)
        writer.Flush()
        stream.Position = 0
        Return stream
    End Function

    ''' <summary>
    ''' Aplica uma mascara a um numero de telefone
    ''' </summary>
    ''' <param name="Number"></param>
    ''' <returns></returns>
    <Extension> Public Function ToTelephone(Number As String) As String
        Number = If(Number, "")
        Dim mask As String = ""
        Number = Number.ParseDigits.RemoveAny(",", ".")
        If Number.IsBlank Then
            Return ""
        End If
        Select Case Number.Length
            Case Is <= 4
                mask = "{0:####}"
            Case Is <= 8
                mask = "{0:####-####}"
            Case 9
                mask = "{0:#####-####}"
            Case 10
                mask = "{0:(##) ####-####}"
            Case 11
                mask = "{0:(##) #####-####}"
            Case 12
                mask = "{0:+## (##) ####-####}"
            Case 13
                mask = "{0:+## (##) #####-####}"
            Case Else
                Return Number.ToString
        End Select
        Return String.Format(mask, Long.Parse(Number))
    End Function

    ''' <summary>
    ''' Aplica uma mascara a um numero de telefone
    ''' </summary>
    ''' <param name="Number"></param>
    ''' <returns></returns>
    <Extension> Public Function ToTelephone(Number As Long) As String
        Return Number.ToString.ToTelephone
    End Function

    ''' <summary>
    ''' Aplica uma mascara a um numero de telefone
    ''' </summary>
    ''' <param name="Number"></param>
    ''' <returns></returns>
    <Extension> Public Function ToTelephone(Number As Integer) As String
        Return Number.ToString.ToTelephone
    End Function

    ''' <summary>
    ''' Aplica uma mascara a um numero de telefone
    ''' </summary>
    ''' <param name="Number"></param>
    ''' <returns></returns>
    <Extension> Public Function ToTelephone(Number As Decimal) As String
        Return Number.ToString.ToTelephone
    End Function

    ''' <summary>
    ''' Aplica uma mascara a um numero de telefone
    ''' </summary>
    ''' <param name="Number"></param>
    ''' <returns></returns>
    <Extension> Public Function ToTelephone(Number As Double) As String
        Return Number.ToString.ToTelephone
    End Function

    ''' <summary>
    ''' Transforma um texto em titulo
    ''' </summary>
    ''' <param name="Text">Texto a ser manipulado</param>
    ''' <returns>Uma String com o texto em nome próprio</returns>
    <Extension()>
    Public Function ToTitle(Text As String) As String
        Return ToProper(Text)
    End Function


    ''' <summary>
    ''' Transforma um XML Document em string
    ''' </summary>
    ''' <param name="XML">Documento XML</param>
    ''' <returns></returns>
    <Extension()>
    Public Function ToXMLString(XML As XmlDocument) As String
        Using stringWriter = New StringWriter()
            Using xmlTextWriter = XmlWriter.Create(stringWriter)
                XML.WriteTo(xmlTextWriter)
                xmlTextWriter.Flush()
                Return stringWriter.GetStringBuilder().ToString()
            End Using
        End Using
    End Function

    ''' <summary>
    ''' Remove do começo e do final de uma string qualquer valor que estiver no conjunto
    ''' </summary>
    ''' <param name="Text">              Texto</param>
    ''' <param name="ContinuouslyRemove">
    ''' Parametro que indica se a string deve continuar sendo testada até que todas as ocorrencias
    ''' sejam removidas
    ''' </param>
    ''' <param name="StringTest">        Conjunto de textos que serão comparados</param>
    ''' <returns></returns>
    <Extension()> Public Function TrimAny(ByVal Text As String, ContinuouslyRemove As Boolean, ParamArray StringTest As String()) As String
        Text = Text.RemoveFirstAny(ContinuouslyRemove, StringTest)
        Text = Text.RemoveLastAny(ContinuouslyRemove, StringTest)
        Return Text
    End Function

    ''' <summary>
    ''' Remove do começo e do final de uma string qualquer valor que estiver no conjunto
    ''' </summary>
    ''' <param name="Text">      Texto</param>
    ''' <param name="StringTest">Conjunto de textos que serão comparados</param>
    ''' <returns></returns>
    <Extension()> Public Function TrimAny(ByVal Text As String, ParamArray StringTest As String()) As String
        Return Text.TrimAny(True, StringTest)
    End Function

    ''' <summary>
    ''' Remove continuamente caracteres em branco do começo e fim de uma string incluindo breaklines
    ''' </summary>
    ''' <param name="Text"></param>
    ''' <returns></returns>
    <Extension()> Function TrimCarriage(Text As String) As String
        Return Text.TrimAny(WhiteSpaceChars)
    End Function

    ''' <summary>
    ''' Decoda uma string de uma transmissão por URL
    ''' </summary>
    ''' <param name="Text">Texto</param>
    ''' <returns></returns>
    <Extension()>
    Public Function UrlDecode(ByVal Text As String) As String
        Return HttpUtility.UrlDecode("" & Text)
    End Function

    ''' <summary>
    ''' Encoda uma string para transmissão por URL
    ''' </summary>
    ''' <param name="Text">Texto</param>
    ''' <returns></returns>
    <Extension()>
    Public Function UrlEncode(ByVal Text As String) As String
        Return HttpUtility.UrlEncode("" & Text)
    End Function

    ''' <summary>
    ''' Encapsula um tento entre 2 textos
    ''' </summary>
    ''' <param name="Text">    Texto</param>
    ''' <param name="WrapText">Caractere de encapsulamento</param>
    ''' <returns></returns>
    <Extension()>
    Function Wrap(Text As String, Optional WrapText As String = """") As String
        If Text.IsNotBlank Then
            Return WrapText & Text & WrapText
        End If
        Return Text
    End Function

    ''' <summary>
    ''' Encapsula um texto dentro de um elemento HTML
    ''' </summary>
    ''' <param name="Text">   Texto</param>
    ''' <param name="TagName">Nome da Tag (Exemplo: div)</param>
    ''' <returns>Uma string HTML com seu texto dentro de uma tag</returns>
    <Extension>
    Function WrapInTag(Text As String, TagName As String) As HtmlParser.HtmlElement
        TagName = TagName.RemoveAny("<", ">", "/").ToLower()
        Return New HtmlParser.HtmlElement(TagName, Text)
    End Function

    <Extension()>
    Private Function InExtensive(ByVal Number As Long) As String

        Select Case Number
            Case Is < 0
                Return "Menos " & InExtensive(Number * (-1))
            Case 0
                Return "Zero"
            Case 1 To 19
                Dim strArray() As String =
                    {"Um", "Dois", "Três", "Quatro", "Cinco", "Seis", "Sete", "Oito", "Nove", "Dez", "Onze", "Doze",
                    "Treze", "Quatorze", "Quinze", "Dezesseis", "Dezessete", "Dezoito", "Dezenove"}
                Return strArray(Number - 1) + " "
            Case 20 To 99
                Dim strArray() As String = {"Vinte", "Trinta", "Quarenta", "Cinquenta", "Sessenta", "Setenta", "Oitenta", "Noventa"}
                If (Number Mod 10) = 0 Then
                    Return strArray(Number \ 10 - 2)
                Else
                    Return strArray(Number \ 10 - 2) + " e " + InExtensive(Number Mod 10)
                End If
            Case 100
                Return "Cem"
            Case 101 To 999
                Dim strArray() As String = {"Cento", "Duzentos", "Trezentos", "Quatrocentos", "Quinhentos", "Seiscentos", "Setecentos", "Oitocentos", "Novecentos"}
                If (Number Mod 100) = 0 Then
                    Return strArray(Number \ 100 - 1) + " "
                Else
                    Return strArray(Number \ 100 - 1) + " e " + InExtensive(Number Mod 100)
                End If
            Case 1000 To 1999
                Select Case (Number Mod 1000)
                    Case 0
                        Return "Mil"
                    Case Is <= 100, 200, 300, 400, 500, 600, 700, 800, 900
                        Return "Mil e " & InExtensive(Number Mod 1000)
                    Case Else
                        Return "Mil  " & InExtensive(Number Mod 1000)
                End Select
            Case 2000 To 999999
                Select Case (Number Mod 1000)
                    Case 0
                        Return InExtensive(Number \ 1000) & " Mil"
                    Case Is <= 100
                        Return InExtensive(Number \ 1000) & " Mil e " & InExtensive(Number Mod 1000)
                    Case Else
                        Return InExtensive(Number \ 1000) & " Mil " & InExtensive(Number Mod 1000)
                End Select

#Region "Milhao"

            Case 1000000 To 1999999
                Select Case (Number Mod 1000000)
                    Case 0
                        Return "Um Milhão "
                    Case Is <= 100, 200, 300, 400, 500, 600, 700, 800, 900
                        Return "Um Milhão e " & InExtensive(Number Mod 1000000)
                    Case Else
                        Return "Um Milhão  " & InExtensive(Number Mod 1000000)
                End Select
            Case 2000000 To 999999999
                Select Case (Number Mod 1000000)
                    Case 0
                        Return InExtensive(Number \ 1000000) & " Milhões "
                    Case Is <= 100, 200, 300, 400, 500, 600, 700, 800, 900
                        Return InExtensive(Number \ 1000000) & " Milhões e " & InExtensive(Number Mod 1000000)
                    Case Else
                        Return InExtensive(Number \ 1000000) & " Milhões " & InExtensive(Number Mod 1000000)
                End Select

#End Region

#Region "Bilhao"

            Case 1000000000 To 1999999999
                Select Case (Number Mod 1000000000)
                    Case 0
                        Return "Um Bilhão "
                    Case Is <= 100, 200, 300, 400, 500, 600, 700, 800, 900
                        Return "Um Bilhão e " & InExtensive(Number Mod 1000000000)
                    Case Else
                        Return "Um Bilhão  " & InExtensive(Number Mod 1000000000)
                End Select
            Case 2000000000 To 999999999999
                Select Case (Number Mod 1000000000)
                    Case 0
                        Return InExtensive(Number \ 1000000000) & " Bilhões "
                    Case Is <= 100, 200, 300, 400, 500, 600, 700, 800, 900
                        Return InExtensive(Number \ 1000000000) & " Bilhões e " & InExtensive(Number Mod 1000000000)
                    Case Else
                        Return InExtensive(Number \ 1000000000) & " Bilhões " & InExtensive(Number Mod 1000000000)
                End Select

#End Region

#Region "Trilhao"

            Case 1000000000000 To 1999999999999
                Select Case (Number Mod 1000000000000)
                    Case 0
                        Return "Um Trilhão "
                    Case Is <= 100, 200, 300, 400, 500, 600, 700, 800, 900
                        Return "Um Trilhão e " & InExtensive(Number Mod 1000000000000)
                    Case Else
                        Return "Um Trilhão  " & InExtensive(Number Mod 1000000000000)
                End Select
            Case 2000000000000 To 999999999999999
                Select Case (Number Mod 1000000000000)
                    Case 0
                        Return InExtensive(Number \ 1000000000000) & " Trilhões "
                    Case Is <= 100, 200, 300, 400, 500, 600, 700, 800, 900
                        Return InExtensive(Number \ 1000000000000) & " Trilhões e " & InExtensive(Number Mod 1000000000000)
                    Case Else
                        Return InExtensive(Number \ 1000000000000) & " Trilhões " & InExtensive(Number Mod 1000000000000)
                End Select

#End Region

            Case Else
                Throw New NotSupportedException("O número nao pode ser maior que 999 trilhões ")
        End Select

    End Function

End Module