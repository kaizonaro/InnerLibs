﻿Imports System.Collections.Specialized
Imports System.ComponentModel
Imports System.Globalization
Imports System.Runtime.CompilerServices

Imports InnerLibs.LINQ

Public Module Converter

    ''' <summary>
    ''' Cria uma lista vazia usando um objeto como o tipo da lista. Util para tipos anonimos
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="ObjectForDefinition">Objeto que definirá o tipo da lista</param>
    ''' <returns></returns>
    <Extension()> Function DefineEmptyList(Of T)(ObjectForDefinition As T) As List(Of T)
        Return DefineEmptyList(Of T)()
    End Function

    ''' <summary>
    ''' Cria uma lista vazia usando um objeto como o tipo da lista. Util para tipos anonimos
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <returns></returns>
    Function DefineEmptyList(Of T)() As List(Of T)
        Return New List(Of T)
    End Function

    ''' <summary>
    ''' Cria uma e adciona um objeto a ela. Util para tipos anonimos
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <returns></returns>
    <Extension()> Function StartList(Of T)(ObjectForDefinition As T) As List(Of T)
        Dim d = DefineEmptyList(Of T)()
        If ObjectForDefinition IsNot Nothing Then
            d.Add(ObjectForDefinition)
        End If
        Return d
    End Function

    ''' <summary>
    ''' Verifica se um objeto é um array, e se negativo, cria um array de um unico item com o valor do objeto
    ''' </summary>
    ''' <param name="Obj">Objeto</param>
    ''' <returns></returns>
    Public Function ForceArray(Obj As Object) As Object()
        Return ForceArray(Of Object)(Obj)
    End Function

    ''' <summary>
    ''' Verifica se um objeto é um array, e se não, cria um array com oeste objeto
    ''' </summary>
    ''' <param name="Obj">Objeto</param>
    ''' <returns></returns>
    Public Function ForceArray(Of OutputType)(ByVal Obj As Object) As OutputType()
        Dim a As New List(Of OutputType)
        If Obj IsNot Nothing Then
            If Not Obj.GetType().IsArray Then
                If Obj.ToString.IsBlank Then Obj = {} Else Obj = {Obj}
                a.AddRange(ChangeArrayType(Of OutputType, Object)(Obj))
            End If
        End If
        Return a.ToArray
    End Function

    ''' <summary>
    ''' Aplica as mesmas keys a todos os dicionarios de uma lista
    ''' </summary>
    ''' <typeparam name="TKey">Tipo da key</typeparam>
    ''' <typeparam name="TValue">Tipo do Valor</typeparam>
    ''' <param name="Dics">Dicionarios</param>
    '''<param name="AditionalKeys">Chaves para serem incluidas nos dicionários mesmo se não existirem em nenhum deles</param>
    <Extension()> Function MergeKeys(Of TKey, TValue)(Dics As IEnumerable(Of Dictionary(Of TKey, TValue)), ParamArray AditionalKeys As TKey()) As IEnumerable(Of Dictionary(Of TKey, TValue))
        AditionalKeys = If(AditionalKeys, {})
        Dim chave = Dics.SelectMany(Function(x) x.Keys).Distinct.Union(AditionalKeys)
        For Each dic In Dics
            For Each key In chave
                If Not dic.ContainsKey(key) Then
                    dic(key) = Nothing
                End If
            Next
        Next
        Return Dics
    End Function

    ''' <summary>
    ''' Converte um tipo para Boolean. Retorna Nothing (NULL) se a conversão falhar
    ''' </summary>
    ''' <typeparam name="FromType">Tipo de origem</typeparam>
    ''' <param name="Value">Variavel com valor</param>
    ''' <returns>Valor convertido em novo tipo</returns>
    <Extension>
    Public Function ToBoolean(Of FromType)(Value As FromType) As Boolean
        Return Value.ChangeType(Of Boolean)
    End Function

    ''' <summary>
    ''' Converte um tipo para Integer. Retorna Nothing (NULL) se a conversão falhar
    ''' </summary>
    ''' <typeparam name="FromType">Tipo de origem</typeparam>
    ''' <param name="Value">Variavel com valor</param>
    ''' <returns>Valor convertido em novo tipo</returns>
    <Extension>
    Public Function ToInteger(Of FromType)(Value As FromType) As Integer
        Return Value.ChangeType(Of Integer)
    End Function

    ''' <summary>
    ''' Converte um tipo para Decimal. Retorna Nothing (NULL) se a conversão falhar
    ''' </summary>
    ''' <typeparam name="FromType">Tipo de origem</typeparam>
    ''' <param name="Value">Variavel com valor</param>
    ''' <returns>Valor convertido em novo tipo</returns>
    <Extension>
    Public Function ToDecimal(Of FromType)(Value As FromType) As Decimal
        Return Value.ChangeType(Of Decimal)
    End Function

    ''' <summary>
    ''' Converte um tipo para DateTime. Retorna Nothing (NULL) se a conversão falhar
    ''' </summary>
    ''' <typeparam name="FromType">Tipo de origem</typeparam>
    ''' <param name="Value">Variavel com valor</param>
    ''' <returns>Valor convertido em novo tipo</returns>
    <Extension>
    Public Function ToDateTime(Of FromType)(Value As FromType) As DateTime
        Return Value.ChangeType(Of DateTime)
    End Function

    ''' <summary>
    ''' Converte um tipo para DateTime. Retorna Nothing (NULL) se a conversão falhar
    ''' </summary>
    ''' <typeparam name="FromType">Tipo de origem</typeparam>
    ''' <param name="Value">Variavel com valor</param>
    ''' <returns>Valor convertido em novo tipo</returns>
    <Extension>
    Public Function ToDateTime(Of FromType)(Value As FromType, CultureInfoName As String) As DateTime
        Return Value.ToDateTime(New CultureInfo(CultureInfoName))
    End Function

    ''' <summary>
    ''' Converte um tipo para DateTime. Retorna Nothing (NULL) se a conversão falhar
    ''' </summary>
    ''' <typeparam name="FromType">Tipo de origem</typeparam>
    ''' <param name="Value">Variavel com valor</param>
    ''' <returns>Valor convertido em novo tipo</returns>
    <Extension>
    Public Function ToDateTime(Of FromType)(Value As FromType, CultureInfo As CultureInfo) As DateTime
        Return Convert.ToDateTime(Value, CultureInfo)
    End Function

    ''' <summary>
    ''' Converte um tipo para Double. Retorna Nothing (NULL) se a conversão falhar
    ''' </summary>
    ''' <typeparam name="FromType">Tipo de origem</typeparam>
    ''' <param name="Value">Variavel com valor</param>
    ''' <returns>Valor convertido em novo tipo</returns>
    <Extension>
    Public Function ToDouble(Of FromType)(Value As FromType) As Double
        Return Value.ChangeType(Of Double)
    End Function

    ''' <summary>
    ''' Converte um tipo para Integer. Retorna Nothing (NULL) se a conversão falhar
    ''' </summary>
    ''' <typeparam name="FromType">Tipo de origem</typeparam>
    ''' <param name="Value">Variavel com valor</param>
    ''' <returns>Valor convertido em novo tipo</returns>
    <Extension>
    Public Function ToLong(Of FromType)(Value As FromType) As Long
        Return Value.ChangeType(Of Long)
    End Function

    ''' <summary>
    ''' Converte um tipo para outro. Retorna Nothing (NULL) se a conversão falhar
    ''' </summary>
    ''' <typeparam name="ToType">Tipo</typeparam>
    ''' <typeparam name="FromType">Tipo de origem</typeparam>
    ''' <param name="Value">Variavel com valor</param>
    ''' <returns>Valor convertido em novo tipo ou null se a conversão falhar</returns>
    <Extension>
    Public Function ChangeType(Of ToType, FromType)(Value As FromType) As ToType
        Return ChangeType(Of FromType)(Value, GetType(ToType))
    End Function

    ''' <summary>
    ''' Converte um tipo para outro. Retorna Nothing (NULL) se a conversão falhar
    ''' </summary>
    ''' <typeparam name="FromType">Tipo de origem</typeparam>
    ''' <param name="Value">Variavel com valor</param>
    ''' <returns>Valor convertido em novo tipo ou null se a conversão falhar</returns>
    <Extension>
    Public Function ChangeType(Of FromType)(Value As FromType, ToType As Type)
        Try
            Dim tipo As Type = If(Nullable.GetUnderlyingType(ToType), ToType)

            If Value Is Nothing Then
                Return Nothing
            End If

            Dim Converter = TypeDescriptor.GetConverter(tipo)

            If Converter.CanConvertFrom(GetType(FromType)) Then
                Return Converter.ConvertTo(Value, tipo)
            End If

            Return Convert.ChangeType(Value, tipo)
        Catch ex As Exception
            Debug.WriteLine(ex)
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Converte um array de um tipo para outro
    ''' </summary>
    ''' <typeparam name="ToType">Tipo do array</typeparam>
    ''' <param name="Value">Array com elementos</param>
    ''' <returns>Array convertido em novo tipo</returns>
    <Extension>
    Public Function ChangeArrayType(Of ToType, FromType)(Value As FromType()) As ToType()
        Dim d As New List(Of ToType)
        If Value.Count > 0 Then
            For Each el As FromType In Value
                d.Add(el.ChangeType(Of ToType))
            Next
            Return d.ToArray
        End If
        Return {}
    End Function

    ''' <summary>
    ''' Converte um IEnumerable de um tipo para outro
    ''' </summary>
    ''' <typeparam name="ToType">Tipo do array</typeparam>
    ''' <param name="Value">Array com elementos</param>
    ''' <returns>Array convertido em novo tipo</returns>
    <Extension>
    Public Function ChangeIEnumerableType(Of ToType, FromType)(Value As IEnumerable(Of FromType)) As IEnumerable(Of ToType)
        Dim d As New List(Of ToType)
        If Value.Count > 0 Then
            For Each el As FromType In Value
                d.Add(el.ChangeType(Of ToType))
            Next
            Return d.AsEnumerable
        End If
        Return {}
    End Function

    ''' <summary>
    ''' Mescla varios dicionarios em um unico dicionario. Quando uma key existir em mais de um dicionario os valores sao agrupados em arrays
    ''' </summary>
    ''' <typeparam name="Tkey">Tipo da Key, Deve ser igual para todos os dicionarios</typeparam>
    ''' <param name="FirstDictionary">Dicionario Principal</param>
    ''' <param name="Dictionaries">Outros dicionarios</param>
    ''' <returns></returns>

    <Extension()> Function Merge(Of Tkey)(FirstDictionary As Dictionary(Of Tkey, Object), ParamArray Dictionaries As Dictionary(Of Tkey, Object)()) As Dictionary(Of Tkey, Object)

        'dicionario que está sendo gerado a partir dos outros
        Dim result As New Dictionary(Of Tkey, Object)

        'adiciona o primeiro dicionario ao array principal e exclui dicionarios vazios
        Dictionaries = Dictionaries.Union({FirstDictionary}).Where(Function(x) x.Count > 0).ToArray

        'cria um array de keys unicas a partir de todos os dicionarios
        Dim keys = Dictionaries.SelectMany(Function(x) x.Keys.ToArray).Distinct

        'para cada chave encontrada
        For Each key In keys
            'para cada dicionario a ser mesclado
            For Each dic In Dictionaries
                'dicionario tem a chave?
                If dic.ContainsKey(key) Then
                    'resultado ja tem a chave atual adicionada?
                    If result.ContainsKey(key) Then
                        'lista que vai mesclar tudo
                        Dim lista As New List(Of Object)

                        'chave do resultado é um array?
                        If IsArray(result(key)) Then
                            lista.AddRange(result(key))
                        Else
                            lista.Add(result(key))
                        End If
                        'chave do dicionario é um array?
                        If IsArray(dic(key)) Then
                            lista.AddRange(dic(key))
                        Else
                            lista.Add(dic(key))
                        End If

                        'transforma a lista em um resultado
                        If lista.Count > 0 Then
                            If lista.Count > 1 Then
                                result(key) = lista.ToArray
                            Else
                                result(key) = lista.First
                            End If
                        End If
                    Else
                        If dic(key).GetType IsNot GetType(String) AndAlso (IsArray(dic(key)) OrElse IsList(dic(key))) Then
                            result.Add(key, dic(key).ToArray)
                        Else
                            result.Add(key, dic(key))
                        End If
                    End If
                End If
            Next
        Next
        Return result

    End Function

    ''' <summary>
    ''' Returna um <see cref=" Dictionary"/> a partir de um <see cref="IGrouping(Of TKey, TElement)"/>
    ''' </summary>
    ''' <typeparam name="TKey"></typeparam>
    ''' <typeparam name="TValue"></typeparam>
    ''' <param name="groupings"></param>
    ''' <returns></returns>
    <Extension()>
    Public Function ToDictionary(Of TKey, TValue)(ByVal groupings As IEnumerable(Of IGrouping(Of TKey, TValue))) As Dictionary(Of TKey, IEnumerable(Of TValue))
        Return groupings.ToDictionary(Function(group) group.Key, Function(group) group.AsEnumerable)
    End Function

    ''' <summary>
    ''' Seta as propriedades de uma classe a partir de um dictionary
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="Dic"></param>
    ''' <param name="Obj"></param>
    <Extension()>
    Public Sub SetPropertiesIn(Of T As Class)(Dic As IDictionary(Of String, Object), Obj As T)
        For Each k In Dic
            If Obj.HasProperty(k.Key) Then
                Obj.SetPropertyValue(k.Key, k.Value)
            End If
        Next
    End Sub

    ''' <summary>
    ''' Transforma uma lista de pares em um Dictionary
    ''' </summary>
    ''' <typeparam name="TKey"></typeparam>
    ''' <typeparam name="TValue"></typeparam>
    ''' <param name="items"></param>
    ''' <returns></returns>
    <Extension()> Public Function ToDictionary(Of TKey, TValue)(items As IEnumerable(Of KeyValuePair(Of TKey, TValue))) As Dictionary(Of TKey, TValue)
        Return items.DistinctBy(Function(x) x.Key).ToDictionary(Of TKey, TValue)(Function(x) x.Key, Function(x) x.Value)
    End Function

    ''' <summary>
    ''' Converte um NameValueCollection para um <see cref="Dictionary(Of String, Object)"/>
    ''' </summary>
    ''' <param name="[NameValueCollection]">Formulario</param>
    ''' <returns></returns>
    <Extension>
    Public Function ToDictionary([NameValueCollection] As NameValueCollection, ParamArray Keys As String()) As Dictionary(Of String, Object)
        Dim result = New Dictionary(Of String, Object)()
        If If(Keys, {}).LongCount = 0 Then Keys = NameValueCollection.AllKeys
        For Each key As String In [NameValueCollection].Keys
            If key.IsNotBlank AndAlso key.IsLikeAny(Keys) Then
                Dim values As String() = [NameValueCollection].GetValues(key)
                If result.ContainsKey(key) Then
                    Dim l As New List(Of Object)
                    If IsArray(result(key)) Then
                        For Each v In result(key)
                            Select Case True
                                Case IsNumber(v)
                                    l.Add(Convert.ToDouble(v))
                                    Exit Select

                                Case IsDate(v)
                                    l.Add(Convert.ToDateTime(v))
                                    Exit Select

                                Case Else
                                    l.Add(v)
                            End Select
                        Next
                    Else
                        Select Case True
                            Case Verify.IsNumber(result(key))
                                l.Add(Convert.ToDouble(result(key)))
                                Exit Select

                            Case IsDate(result(key))
                                Exit Select

                                l.Add(Convert.ToDateTime(result(key)))
                            Case Else
                                l.Add(result(key))
                        End Select
                    End If
                    If l.Count = 1 Then
                        result(key) = l(0)
                    Else
                        result(key) = l.ToArray
                    End If
                Else
                    If values.Length = 1 Then
                        Select Case True
                            Case Verify.IsNumber(values(0))
                                result.Add(key, Convert.ToDouble(values(0)))
                                Exit Select
                            Case IsDate(values(0))
                                result.Add(key, Convert.ToDateTime(values(0)))
                                Exit Select
                            Case Else
                                result.Add(key, values(0))
                        End Select
                    Else
                        Dim ar As New List(Of Object)
                        For Each v In values
                            Select Case True
                                Case Verify.IsNumber(v)
                                    ar.Add(Convert.ToDouble(v))
                                    Exit Select

                                Case IsDate(v)
                                    ar.Add(Convert.ToDateTime(v))
                                    Exit Select

                                Case Else
                                    ar.Add(v)
                            End Select
                        Next
                        result.Add(key, ar.ToArray)
                    End If
                End If

            End If
        Next

        Return result
    End Function

End Module