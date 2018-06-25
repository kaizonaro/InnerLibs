﻿Imports System.Globalization
Imports InnerLibs

''' <summary>
''' Estrutura que representa valores em dinheiro de uma determinada <see cref="CultureInfo"/>. Utiliza uma API (http://fixer.io) para conversão de moedas.
''' </summary>
Public Structure Money

    Public Property Value As Decimal

    ''' <summary>
    ''' Cria uma nova instancia de moeda
    ''' </summary>
    ''' <param name="Value">Valor</param>
    Sub New(Optional Value As Decimal = 0)
        Me.New(Value, CultureInfo.CurrentCulture)
    End Sub

    ''' <summary>
    ''' Cria uma nova instancia de moeda
    ''' </summary>
    ''' <param name="Value">Valor</param>
    ''' <param name="Culture">Cultura</param>
    Public Sub New(Value As Decimal, Culture As CultureInfo)
        Me.Value = Value
        Me.ISOCurrencySymbol = New RegionInfo(Culture.Name).ISOCurrencySymbol
        Me.CurrencySymbol = Culture.NumberFormat.CurrencySymbol
    End Sub

    ''' <summary>
    ''' Cria uma nova instancia de moeda
    ''' </summary>
    ''' <param name="Value">Valor</param>
    ''' <param name="ISOCurrencySymbol">Simbolo de moeda, ISO ou nome da cultura</param>
    Public Sub New(Value As Decimal, ISOCurrencySymbol As String, Optional CurrencySymbol As String = "")
        Me.Value = Value
        Dim c = GetCultureInfosByCurrencySymbol(ISOCurrencySymbol).FirstOrDefault
        If c.Equals(CultureInfo.InvariantCulture) Then
            Me.CurrencySymbol = CurrencySymbol.IfBlank(ISOCurrencySymbol)
            Me.ISOCurrencySymbol = ISOCurrencySymbol
        Else
            Me.CurrencySymbol = New RegionInfo(c.Name).CurrencySymbol
            Me.ISOCurrencySymbol = New RegionInfo(c.Name).ISOCurrencySymbol
        End If
    End Sub



    ''' <summary>
    ''' Converte de uma moeda para a outra utilizando a api http://cryptonator.com
    ''' </summary>
    ''' <param name="Symbol">Simbolo de moeda, ISO ou nome da cultura</param>
    ''' <returns></returns>
    Function ConvertCurrency(Symbol As String) As Money
        Dim cult = GetCultureInfosByCurrencySymbol(Symbol).FirstOrDefault
        If cult.Equals(CultureInfo.InvariantCulture) Then
            Return New Money(ConvertMoney(Symbol), Symbol)
        Else
            Return ConvertCurrency(cult)
        End If
    End Function

    ''' <summary>
    ''' Converte de uma moeda para a outra utilizando a api http://cryptonator.com
    ''' </summary>
    ''' <param name="Culture">Cultura</param>
    ''' <returns></returns>
    Function ConvertCurrency(Culture As CultureInfo) As Money
        If Me.ISOCurrencySymbol.ToLower = New RegionInfo(Culture.Name).ISOCurrencySymbol.ToLower Then Return New Money(Me.Value, Culture)
        Return New Money(ConvertMoney(New RegionInfo(Culture.Name).ISOCurrencySymbol.ToLower), Culture)
    End Function

    Private Function ConvertMoney(ToSymbol As String) As Decimal
        If Not IsConnected() Then
            Throw New Exception("Internet is not available to convert currency.")
        End If
        Dim rep = AJAX.GET(Of Object)("https://api.cryptonator.com/api/ticker/" & Me.ISOCurrencySymbol & "-" & ToSymbol.ToLower)

        Return Me.Value * Convert.ToDecimal(rep("ticker")("price").ToString, New CultureInfo("en-US"))
    End Function


    ''' <summary>
    ''' String do valor formatado como moeda, é um alias para <see cref="MoneyString"/>
    ''' </summary>
    ''' <returns></returns>
    Public Overrides Function ToString() As String
        Return MoneyString
    End Function

    ''' <summary>
    ''' String do valor formatado como moeda
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property MoneyString As String
        Get
            Return Me.ToString(0)
        End Get
    End Property

    ''' <summary>
    ''' String do valor formatado como moeda
    ''' </summary>
    ''' <param name="Precision">Precisao de casas decimais</param>
    ''' <returns></returns>
    Public Overloads Function ToString(Precision As Integer)
        Dim c = GetCultureInfosByCurrencySymbol(Me.ISOCurrencySymbol).First
        If c.Equals(CultureInfo.InvariantCulture) Then
            c = CultureInfo.CurrentCulture
        End If
        Dim ss = CurrencySymbol & " " & If(Precision > 0, Me.Value.Slice(Precision.SetMinValue(2)), Me.Value).ToString(c)
        If Not (Me.Value Mod 1) = 0 AndAlso Me.Value > 0 Then
            While ss.EndsWith("0")
                ss = ss.TrimEnd("0")
            End While
        End If
        Return ss.IfBlank("0")
    End Function

    ''' <summary>
    ''' Simbolo de moeda
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property CurrencySymbol As String


    ''' <summary>
    ''' Simbolo de moeda utilizada em cambio (ISO)
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property ISOCurrencySymbol As String


    ''' <summary>
    ''' Traz uma lista de <see cref="CultureInfo"/> que utilizam uma determinada moeda de acordo com o simbolo, simbolo ISO ou
    ''' </summary>
    ''' <param name="Currency">Moeda</param>
    ''' <returns></returns>
    Public Shared Function GetCultureInfosByCurrencySymbol(Currency As String) As List(Of CultureInfo)
        If Currency Is Nothing OrElse Currency.IsBlank Then
            Throw New ArgumentNullException("Currency is blank")
        End If
        Dim l = CultureInfo.GetCultures(CultureTypes.SpecificCultures) _
        .Where(Function(x) (New RegionInfo(x.LCID).ISOCurrencySymbol.Trim = Currency.Trim Or New RegionInfo(x.LCID).CurrencySymbol.Trim = Currency.Trim Or x.Name.Trim = Currency.Trim)).ToList
        If l.Count = 0 Then l.Add(CultureInfo.InvariantCulture)
        Return l
    End Function




    Public Shared Operator &(Text As String, Value As Money) As String
        Return Text & Value.MoneyString
    End Operator

    Public Shared Operator &(Value As Money, Text As String) As String
        Return Value.MoneyString & Text
    End Operator

    Public Shared Operator Not(Value As Money)
        Return New Money(Value.Value * -1, Value.ISOCurrencySymbol)
    End Operator

    Public Shared Operator +(Text As String, Value As Money) As String
        If Text.IsNumber Then
            Return (Text.ChangeType(Of Decimal) + Value.Value).ToString
        Else
            Return Text & Value.MoneyString
        End If
    End Operator

    Public Shared Operator +(Value As Money, Text As String) As String
        Return Text + Value
    End Operator

    Public Shared Operator +(Value1 As Double, Value2 As Money) As Money
        Return New Money(Value1 + Value2.Value, Value2.ISOCurrencySymbol)
    End Operator

    Public Shared Operator +(Value2 As Money, Value1 As Double) As Money
        Return New Money(Value1 + Value2.Value, Value2.ISOCurrencySymbol)
    End Operator

    Public Shared Operator +(Value1 As Decimal, Value2 As Money) As Money
        Return New Money(Value1 + Value2.Value, Value2.ISOCurrencySymbol)
    End Operator

    Public Shared Operator +(Value2 As Money, Value1 As Decimal) As Money
        Return New Money(Value1 + Value2.Value, Value2.ISOCurrencySymbol)
    End Operator

    Public Shared Operator +(Value1 As Integer, Value2 As Money) As Money
        Return New Money(Value1 + Value2.Value, Value2.ISOCurrencySymbol)
    End Operator

    Public Shared Operator +(Value2 As Money, Value1 As Integer) As Money
        Return New Money(Value1 + Value2.Value, Value2.ISOCurrencySymbol)
    End Operator

    Public Shared Operator +(Value1 As Long, Value2 As Money) As Money
        Return New Money(Value1 + Value2.Value, Value2.ISOCurrencySymbol)
    End Operator

    Public Shared Operator +(Value2 As Money, Value1 As Money) As Money
        If Not Value1.ISOCurrencySymbol = Value2.ISOCurrencySymbol Then
            Value1 = Value1.ConvertCurrency(Value2.ISOCurrencySymbol)
        End If
        Return New Money(Value1.Value + Value2.Value, Value2.ISOCurrencySymbol)
    End Operator

    Public Shared Operator -(Value1 As Double, Value2 As Money) As Money
        Return New Money(Value1 - Value2.Value, Value2.ISOCurrencySymbol)
    End Operator

    Public Shared Operator -(Value2 As Money, Value1 As Double) As Money
        Return New Money(Value1 - Value2.Value, Value2.ISOCurrencySymbol)
    End Operator

    Public Shared Operator -(Value1 As Decimal, Value2 As Money) As Money
        Return New Money(Value1 - Value2.Value, Value2.ISOCurrencySymbol)
    End Operator

    Public Shared Operator -(Value2 As Money, Value1 As Decimal) As Money
        Return New Money(Value1 - Value2.Value, Value2.ISOCurrencySymbol)
    End Operator

    Public Shared Operator -(Value1 As Integer, Value2 As Money) As Money
        Return New Money(Value1 - Value2.Value, Value2.ISOCurrencySymbol)
    End Operator

    Public Shared Operator -(Value2 As Money, Value1 As Integer) As Money
        Return New Money(Value1 - Value2.Value, Value2.ISOCurrencySymbol)
    End Operator

    Public Shared Operator -(Value1 As Long, Value2 As Money) As Money
        Return New Money(Value1 - Value2.Value, Value2.ISOCurrencySymbol)
    End Operator

    Public Shared Operator -(Value1 As Money, Value2 As Money) As Money
        Return New Money(Value1.Value - Value2.Value, Value1.ISOCurrencySymbol)
    End Operator

    Public Shared Operator *(Value1 As Double, Value2 As Money) As Money
        Return New Money(Value1 * Value2.Value, Value2.ISOCurrencySymbol)
    End Operator

    Public Shared Operator *(Value2 As Money, Value1 As Double) As Money
        Return New Money(Value1 * Value2.Value, Value2.ISOCurrencySymbol)
    End Operator

    Public Shared Operator *(Value1 As Decimal, Value2 As Money) As Money
        Return New Money(Value1 * Value2.Value, Value2.ISOCurrencySymbol)
    End Operator

    Public Shared Operator *(Value2 As Money, Value1 As Decimal) As Money
        Return New Money(Value1 * Value2.Value, Value2.ISOCurrencySymbol)
    End Operator

    Public Shared Operator *(Value1 As Integer, Value2 As Money) As Money
        Return New Money(Value1 * Value2.Value, Value2.ISOCurrencySymbol)
    End Operator

    Public Shared Operator *(Value2 As Money, Value1 As Integer) As Money
        Return New Money(Value1 * Value2.Value, Value2.ISOCurrencySymbol)
    End Operator

    Public Shared Operator *(Value1 As Long, Value2 As Money) As Money
        Return New Money(Value1 * Value2.Value, Value2.ISOCurrencySymbol)
    End Operator

    Public Shared Operator *(Value2 As Money, Value1 As Money) As Money
        If Not Value1.ISOCurrencySymbol = Value2.ISOCurrencySymbol Then
            Value1 = Value1.ConvertCurrency(Value2.ISOCurrencySymbol)
        End If
        Return New Money(Value1.Value * Value2.Value, Value2.ISOCurrencySymbol)
    End Operator

    Public Shared Operator /(Value1 As Double, Value2 As Money) As Money
        Return New Money(Value1 / Value2.Value, Value2.ISOCurrencySymbol)
    End Operator

    Public Shared Operator /(Value2 As Money, Value1 As Double) As Money
        Return New Money(Value1 / Value2.Value, Value2.ISOCurrencySymbol)
    End Operator

    Public Shared Operator /(Value1 As Decimal, Value2 As Money) As Money
        Return New Money(Value1 / Value2.Value, Value2.ISOCurrencySymbol)
    End Operator

    Public Shared Operator /(Value2 As Money, Value1 As Decimal) As Money
        Return New Money(Value1 / Value2.Value, Value2.ISOCurrencySymbol)
    End Operator

    Public Shared Operator /(Value1 As Integer, Value2 As Money) As Money
        Return New Money(Value1 / Value2.Value, Value2.ISOCurrencySymbol)
    End Operator

    Public Shared Operator /(Value2 As Money, Value1 As Integer) As Money
        Return New Money(Value1 / Value2.Value, Value2.ISOCurrencySymbol)
    End Operator

    Public Shared Operator /(Value1 As Long, Value2 As Money) As Money
        Return New Money(Value1 / Value2.Value, Value2.ISOCurrencySymbol)
    End Operator

    Public Shared Operator /(Value2 As Money, Value1 As Money) As Money
        If Not Value1.ISOCurrencySymbol = Value2.ISOCurrencySymbol Then
            Value1 = Value1.ConvertCurrency(Value2.ISOCurrencySymbol)
        End If
        Return New Money(Value1.Value / Value2.Value, Value2.ISOCurrencySymbol)
    End Operator

    Public Shared Operator =(Value2 As Money, Value1 As Money) As Boolean
        Return Value1.Value = Value2.ConvertCurrency(Value1.ISOCurrencySymbol)
    End Operator

    Public Shared Operator =(Value1 As Double, Value2 As Money) As Boolean
        Return Value1 = Value2.Value
    End Operator

    Public Shared Operator =(Value2 As Money, Value1 As Double) As Boolean
        Return Value1 = Value2.Value

    End Operator

    Public Shared Operator =(Value1 As Decimal, Value2 As Money) As Boolean
        Return Value1 = Value2.Value

    End Operator

    Public Shared Operator =(Value2 As Money, Value1 As Decimal) As Boolean
        Return Value1 = Value2.Value
    End Operator

    Public Shared Operator =(Value1 As Integer, Value2 As Money) As Boolean
        Return Value1 = Value2.Value

    End Operator

    Public Shared Operator =(Value2 As Money, Value1 As Integer) As Boolean
        Return Value1 = Value2.Value

    End Operator

    Public Shared Operator =(Value1 As Long, Value2 As Money) As Boolean
        Return Value1 = Value2.Value

    End Operator

    Public Shared Operator <>(Value2 As Money, Value1 As Money) As Boolean
        Return Not Value1 = Value2
    End Operator

    Public Shared Operator <>(Value1 As Double, Value2 As Money) As Boolean
        Return Not Value1 = Value2.Value
    End Operator

    Public Shared Operator <>(Value2 As Money, Value1 As Double) As Boolean
        Return Not Value1 = Value2.Value
    End Operator

    Public Shared Operator <>(Value1 As Decimal, Value2 As Money) As Boolean
        Return Not Value1 = Value2.Value
    End Operator

    Public Shared Operator <>(Value2 As Money, Value1 As Decimal) As Boolean
        Return Not Value1 = Value2.Value

    End Operator

    Public Shared Operator <>(Value1 As Integer, Value2 As Money) As Boolean
        Return Not Value1 = Value2.Value

    End Operator

    Public Shared Operator <>(Value2 As Money, Value1 As Integer) As Boolean
        Return Not Value1 = Value2.Value

    End Operator

    Public Shared Operator <>(Value1 As Long, Value2 As Money) As Boolean
        Return Not Value1 = Value2.Value
    End Operator

    Public Shared Operator >=(Value2 As Money, Value1 As Money) As Boolean
        Return Value1.Value >= Value2.ConvertCurrency(Value1.ISOCurrencySymbol).Value
    End Operator

    Public Shared Operator >=(Value1 As Double, Value2 As Money) As Boolean
        Return Value1 >= Value2.Value
    End Operator

    Public Shared Operator >=(Value2 As Money, Value1 As Double) As Boolean
        Return Value1 >= Value2.Value

    End Operator

    Public Shared Operator >=(Value1 As Decimal, Value2 As Money) As Boolean
        Return Value1 >= Value2.Value

    End Operator

    Public Shared Operator >=(Value2 As Money, Value1 As Decimal) As Boolean
        Return Value1 >= Value2.Value
    End Operator

    Public Shared Operator >=(Value1 As Integer, Value2 As Money) As Boolean
        Return Value1 >= Value2.Value

    End Operator

    Public Shared Operator >=(Value2 As Money, Value1 As Integer) As Boolean
        Return Value1 >= Value2.Value

    End Operator

    Public Shared Operator >=(Value1 As Long, Value2 As Money) As Boolean
        Return Value1 >= Value2.Value

    End Operator

    Public Shared Operator <=(Value2 As Money, Value1 As Money) As Boolean
        Return Value1.Value <= Value2.ConvertCurrency(Value1.ISOCurrencySymbol).Value
    End Operator

    Public Shared Operator <=(Value1 As Double, Value2 As Money) As Boolean
        Return Value1 <= Value2.Value
    End Operator

    Public Shared Operator <=(Value2 As Money, Value1 As Double) As Boolean
        Return Value1 <= Value2.Value

    End Operator

    Public Shared Operator <=(Value1 As Decimal, Value2 As Money) As Boolean
        Return Value1 <= Value2.Value

    End Operator

    Public Shared Operator <=(Value2 As Money, Value1 As Decimal) As Boolean
        Return Value1 <= Value2.Value
    End Operator

    Public Shared Operator <=(Value1 As Integer, Value2 As Money) As Boolean
        Return Value1 <= Value2.Value

    End Operator

    Public Shared Operator <=(Value2 As Money, Value1 As Integer) As Boolean
        Return Value1 <= Value2.Value

    End Operator

    Public Shared Operator <=(Value1 As Long, Value2 As Money) As Boolean
        Return Value1 <= Value2.Value

    End Operator

    Public Shared Operator >(Value2 As Money, Value1 As Money) As Boolean
        Return Value1.Value > Value2.ConvertCurrency(Value1.ISOCurrencySymbol).Value
    End Operator

    Public Shared Operator >(Value1 As Double, Value2 As Money) As Boolean
        Return Value1 > Value2.Value
    End Operator

    Public Shared Operator >(Value2 As Money, Value1 As Double) As Boolean
        Return Value1 > Value2.Value

    End Operator

    Public Shared Operator >(Value1 As Decimal, Value2 As Money) As Boolean
        Return Value1 > Value2.Value

    End Operator

    Public Shared Operator >(Value2 As Money, Value1 As Decimal) As Boolean
        Return Value1 > Value2.Value
    End Operator

    Public Shared Operator >(Value1 As Integer, Value2 As Money) As Boolean
        Return Value1 > Value2.Value

    End Operator

    Public Shared Operator >(Value2 As Money, Value1 As Integer) As Boolean
        Return Value1 > Value2.Value

    End Operator

    Public Shared Operator >(Value1 As Long, Value2 As Money) As Boolean
        Return Value1 > Value2.Value

    End Operator

    Public Shared Operator <(Value2 As Money, Value1 As Money) As Boolean
        Return Value1.Value < Value2.ConvertCurrency(Value1.ISOCurrencySymbol).Value
    End Operator

    Public Shared Operator <(Value1 As Double, Value2 As Money) As Boolean
        Return Value1 < Value2.Value
    End Operator

    Public Shared Operator <(Value2 As Money, Value1 As Double) As Boolean
        Return Value1 < Value2.Value

    End Operator

    Public Shared Operator <(Value1 As Decimal, Value2 As Money) As Boolean
        Return Value1 < Value2.Value

    End Operator

    Public Shared Operator <(Value2 As Money, Value1 As Decimal) As Boolean
        Return Value1 < Value2.Value
    End Operator

    Public Shared Operator <(Value1 As Integer, Value2 As Money) As Boolean
        Return Value1 < Value2.Value

    End Operator

    Public Shared Operator <(Value2 As Money, Value1 As Integer) As Boolean
        Return Value1 < Value2.Value

    End Operator

    Public Shared Operator <(Value1 As Long, Value2 As Money) As Boolean
        Return Value1 < Value2.Value
    End Operator

    ''' <summary>
    ''' Verifica se 2 valores sao da mesma moeda
    ''' </summary>
    ''' <param name="Value1"></param>
    ''' <param name="Value2"></param>
    ''' <returns></returns>
    Public Shared Operator Like(Value1 As Money, Value2 As Money) As Boolean
        Return Value1.CurrencySymbol = Value2.CurrencySymbol
    End Operator

    Public Shared Widening Operator CType(v As Money) As Decimal
        Return v.Value
    End Operator

    Public Shared Widening Operator CType(v As Decimal) As Money
        Return New Money(v)
    End Operator
    Public Shared Widening Operator CType(v As Integer) As Money
        Return New Money(v)
    End Operator
    Public Shared Widening Operator CType(v As Short) As Money
        Return New Money(v)
    End Operator
    Public Shared Widening Operator CType(v As Long) As Money
        Return New Money(v)
    End Operator
    Public Shared Widening Operator CType(v As Double) As Money
        Return New Money(v)
    End Operator
End Structure