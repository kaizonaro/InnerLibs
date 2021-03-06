﻿Imports System.Collections.Specialized
Imports System.Linq.Expressions

Namespace LINQ

    ''' <summary>
    ''' Classe para criação de paginação e filtros dinâmicos para listas de classes
    ''' </summary>
    ''' <typeparam name="ClassType"></typeparam>
    Public Class PaginationFilter(Of ClassType As Class, RemapType)

        Sub New()
        End Sub

        ''' <summary>
        ''' Cria uma nova instancia e seta a exclusividade de filtro
        ''' </summary>
        Sub New(RemapExpression As Func(Of ClassType, RemapType))
            Me.RemapExpression = RemapExpression
        End Sub

        Sub New(RemapExpression As Func(Of ClassType, RemapType), Options As Action(Of PaginationFilter(Of ClassType, RemapType)))
            Me.RemapExpression = RemapExpression
            Config(Options)
        End Sub

        Sub New(Options As Action(Of PaginationFilter(Of ClassType, RemapType)))
            Config(Options)
        End Sub

        Friend _filters As New List(Of PropertyFilter(Of ClassType, RemapType))

        Friend param As ParameterExpression = GenerateParameterExpression(Of ClassType)()

        ''' <summary>
        ''' Força o <see cref="IQueryable"/> a executar (sem paginação)
        ''' </summary>
        ''' <returns></returns>
        Public Function Compute() As PaginationFilter(Of ClassType, RemapType)
            Me.Data = Me.Data.ToList().AsEnumerable
            Return Me
        End Function

        ''' <summary>
        ''' Numero da pagina
        ''' </summary>
        ''' <returns></returns>
        Public Property PageNumber As Integer = 1

        ''' <summary>
        ''' Filtros
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Filters As IEnumerable(Of PropertyFilter(Of ClassType, RemapType))
            Get
                Return _filters
            End Get
        End Property

        ''' <summary>
        ''' Parametro utilizado na contrução da expressão lambda
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Parameter As ParameterExpression
            Get
                Return param
            End Get
        End Property

        ''' <summary>
        ''' Expressão binária contendo todos os filtros
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property Filter As BinaryExpression
            Get
                Dim exp As Expression = Nothing
                For Each valor In Filters.Where(Function(x) x.Enabled)
                    If valor IsNot Nothing AndAlso valor.Filter IsNot Nothing Then
                        If exp Is Nothing Then
                            exp = valor.Filter
                        Else
                            If valor.Conditional = FilterConditional.And Then
                                exp = Expression.AndAlso(valor.Filter, exp)
                            Else
                                exp = Expression.OrElse(valor.Filter, exp)
                            End If
                        End If
                    End If
                Next
                Return exp
            End Get
        End Property

        ''' <summary>
        ''' Cria uma querystring com os filtros ativos
        ''' </summary>
        ''' <returns></returns>
        Public Function GetFilterQueryString(Optional ForceEnabled As Boolean = False) As String
            Return Filters.Select(Function(x) x.CreateQueryParameter(ForceEnabled)).Where(Function(x) x.IsNotBlank()).Join("&")
        End Function

        ''' <summary>
        ''' Cria uma querystring com  paginacao e os filtros ativos
        ''' </summary>
        ''' <returns></returns>
        Public Function CreateQueryString(Optional PageNumber? As Integer = Nothing, Optional ForceEnabled As Boolean = False, Optional IncludePageSize As Boolean = False, Optional IncludePaginationOffset As Boolean = False) As String
            Dim l As New List(Of String)
            l.Add(GetFilterQueryString(ForceEnabled))
            l.Add(GetPaginationQueryString(If(PageNumber, Me.PageNumber), IncludePageSize, IncludePaginationOffset))
            Return l.Where(Function(x) x.IsNotBlank()).Join("&")
        End Function

        ''' <summary>
        ''' Seta os parametros utilizados na querystring para a paginação
        ''' </summary>
        ''' <param name="PageNumber"></param>
        ''' <param name="PageSize"></param>
        ''' <param name="PaginationOffset"></param>
        ''' <returns></returns>
        Public Function SetPaginationQueryParameters(PageNumber As String, PageSize As String, PaginationOffset As String) As PaginationFilter(Of ClassType, RemapType)
            Me.PageNumberQueryParameter = PageNumber.IfBlank(NameOf(Me.PageNumber))
            Me.PageSizeQueryParameter = PageSize.IfBlank(NameOf(Me.PageSize))
            Me.PaginationOffsetQueryParameter = PaginationOffset.IfBlank(NameOf(Me.PaginationOffset))
            Return Me
        End Function

        Private pnp, psp, pop As String

        Public Property PageNumberQueryParameter As String
            Get
                Return pnp.IfBlank(NameOf(PageNumber))
            End Get
            Set(value As String)
                pnp = value
            End Set
        End Property

        Public Property PageSizeQueryParameter As String
            Get
                Return psp.IfBlank(NameOf(PageSize))
            End Get
            Set(value As String)
                psp = value
            End Set
        End Property

        Public Property PaginationOffsetQueryParameter As String
            Get
                Return pop.IfBlank(NameOf(PageSize))
            End Get
            Set(value As String)
                pop = value
            End Set
        End Property

        ''' <summary>
        ''' Retorna a parte da querystring usada para paginacao
        ''' </summary>
        ''' <param name="PageNumber"></param>
        ''' <returns></returns>
        Public Function GetPaginationQueryString(Optional PageNumber? As Integer = Nothing, Optional IncludePageSize As Boolean = False, Optional IncludePaginationOffset As Boolean = False) As String
            PageNumber = If(PageNumber, Me.PageNumber)
            If PageNumber > 0 Then
                Dim l As New List(Of String)
                If PageNumber > 1 Then l.Add($"{PageNumberQueryParameter}={PageNumber}")
                If IncludePageSize Then l.Add($"{PageSizeQueryParameter}={PageSize}")
                If IncludePaginationOffset Then l.Add($"{PaginationOffsetQueryParameter}={PaginationOffset}")
                Return l.Join("&")
            End If
            Return ""
        End Function

        Public Function ToNameValueCollection(Optional PageNumber? As Integer = Nothing, Optional ForceEnabled As Boolean = False, Optional IncludePageSize As Boolean = False, Optional IncludePaginationOffset As Boolean = False) As NameValueCollection
            Return CreateQueryString(PageNumber, ForceEnabled, IncludePageSize, IncludePaginationOffset).ParseQueryString()
        End Function

        Public Function ToDictionary(Optional PageNumber? As Integer = Nothing, Optional ForceEnabled As Boolean = False, Optional IncludePageSize As Boolean = False, Optional IncludePaginationOffset As Boolean = False) As Dictionary(Of String, Object)
            Return ToNameValueCollection(PageNumber, ForceEnabled, IncludePageSize, IncludePaginationOffset).ToDictionary()
        End Function

        ''' <summary>
        ''' Retorna uma QueryString que representa este filtro
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return CreateQueryString().ToString()
        End Function

        ''' <summary>
        ''' Cria uma Url com a query string deste filtro
        ''' </summary>
        ''' <param name="Url"></param>
        ''' <param name="PageNumber"></param>
        ''' <param name="ForceEnabled"></param>
        ''' <param name="IncludePageSize"></param>
        ''' <param name="IncludePaginationOffset"></param>
        ''' <returns></returns>
        Public Function CreateUrl(Url As String, Optional PageNumber As Integer? = Nothing, Optional ForceEnabled As Boolean = False, Optional IncludePageSize As Boolean = False, Optional IncludePaginationOffset As Boolean = False) As String
            Dim qs = CreateQueryString(If(PageNumber, Me.PageNumber), ForceEnabled, IncludePageSize, IncludePaginationOffset)
            If Url.IsURL Then
                Dim u = New Uri(Url)
                Url = u.GetLeftPart(UriPartial.Path)
                qs = {u.Query, qs}.Join("&")
                Url = Url
            End If
            Return Url & "?" & qs
        End Function

        ''' <summary>
        ''' Cria uma url a partir de um pattern de Url e concatena a query string
        ''' </summary>
        ''' <param name="UrlPattern"></param>
        ''' <param name="ForceEnabled"></param>
        ''' <returns></returns>
        Public Function CreateUrlFromPattern(UrlPattern As String, Optional PageNumber? As Integer = Nothing, Optional ForceEnabled As Boolean = False, Optional IncludePageSize As Boolean = False, Optional IncludePaginationOffset As Boolean = False) As String
            Dim parametros = UrlPattern.GetAllBetween("{", "}").Select(Function(x) x.GetBefore(":"))

            Dim dic = Me.ToDictionary(PageNumber, ForceEnabled, IncludePageSize, IncludePaginationOffset)

            UrlPattern = UrlPattern.ReplaceUrlParameters(dic)

            Dim querystring = ""
            For Each q In dic
                Dim v = ForceArray(Of String)(q.Value).ToList()
                If v.Any Then
                    If parametros.Contains(q.Key, StringComparer.InvariantCultureIgnoreCase) Then
                        UrlPattern = UrlPattern.Replace($"{{{q.Key}}}", v.FirstOrDefault().IfBlank(""))
                        v.RemoveAt(0)
                    End If
                    If v.Any() Then
                        querystring = {querystring, v.SelectJoin(Function(x) q.Key & "=" & x.IfBlank("").ToString().UrlDecode(), "&")}.Where(Function(x) x.IsNotBlank).Join("&")
                    End If
                End If
            Next
            If querystring.IsNotBlank() Then
                UrlPattern = UrlPattern & "?" & querystring
            End If
            UrlPattern = UrlPattern.RemoveUrlParameters()

            Return UrlPattern
        End Function

        ''' <summary>
        ''' Expressão de remapeamento da coleção
        ''' </summary>
        ''' <returns></returns>
        Public Property RemapExpression As Func(Of ClassType, RemapType)
            Get
                If GetType(ClassType) Is GetType(RemapType) Then
                    Return Nothing
                Else
                    Return remapexp
                End If
            End Get
            Set(value As Func(Of ClassType, RemapType))
                If GetType(ClassType) Is GetType(RemapType) Then
                    remapexp = Nothing
                Else
                    remapexp = value
                End If
            End Set
        End Property

        Private remapexp As Func(Of ClassType, RemapType)

        ''' <summary>
        ''' Fonte de Dados deste filtro
        ''' </summary>
        ''' <returns></returns>
        Public Property Data As IEnumerable(Of ClassType) = Nothing

        ''' <summary>
        ''' Dados da Pagina Atual
        ''' </summary>
        ''' <param name="PageNumber"></param>
        ''' <returns></returns>
        Default ReadOnly Property Page(PageNumber As Integer) As RemapType()
            Get
                Return GetPage(PageNumber)
            End Get
        End Property

        ''' <summary>
        ''' Expressão lambda deste filtro
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property LambdaExpression As Expression(Of Func(Of ClassType, Boolean))
            Get
                Dim exp As Expression(Of Func(Of ClassType, Boolean)) = Nothing
                If Filter IsNot Nothing Then
                    exp = Expression.Lambda(Of Func(Of ClassType, Boolean))(Filter, param)
                End If
                For Each valor In If(WhereFilters, New List(Of Expression(Of Func(Of ClassType, Boolean))))
                    If valor IsNot Nothing Then
                        If exp Is Nothing Then
                            exp = valor
                        Else
                            exp = LINQExtensions.And(exp, valor)
                        End If
                    End If
                Next
                If exp IsNot Nothing Then
                    While exp.CanReduce
                        exp = exp.Reduce()
                    End While
                End If
                Return exp
            End Get
        End Property

        ''' <summary>
        ''' Numero da ultima pagina
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property LastPage As Integer
            Get
                Return PageCount
            End Get
        End Property

        ''' <summary>
        ''' Numero da primeira pagina
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property FirstPage As Integer
            Get
                Return 1
            End Get
        End Property

        ''' <summary>
        ''' Numero da proxima pagina
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property NextPage As Integer
            Get
                Dim pp = PageNumber + 1
                If pp > LastPage Then
                    pp = FirstPage
                End If
                Return pp
            End Get
        End Property

        ''' <summary>
        ''' Numero da pagina anterior
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property PreviousPage As Integer
            Get
                Dim pp = PageNumber - 1
                If pp < 1 Then
                    pp = LastPage
                End If
                Return pp
            End Get
        End Property

        ''' <summary>
        ''' Retorna true se esta pagina é a primeira
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property IsFirstPage As Boolean
            Get
                Return PageNumber = FirstPage
            End Get
        End Property

        ''' <summary>
        ''' Retorna true se esta pagina é a ultima
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property IsLastPage As Boolean
            Get
                Return PageNumber = LastPage
            End Get
        End Property

        ''' <summary>
        ''' Retorna true se existir mais de uma pagina
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property IsPaginationNecessary As Boolean
            Get
                Return PageCount > 1
            End Get
        End Property

        ''' <summary>
        ''' Retorna true se existir o botão de primeira página for necessário
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property IsFirstPageNecessary As Boolean
            Get
                Return PageNumber > FirstPage
            End Get
        End Property

        ''' <summary>
        ''' Retorna true se existir o botão de primeira página for necessário
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property IsLastPageNecessary As Boolean
            Get
                Return PageNumber < LastPage
            End Get
        End Property

        ''' <summary>
        ''' Retorna true se existir o botão de pagina anterior for necessário
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property IsPreviousPageNecessary As Boolean
            Get
                Return IsFirstPageNecessary
            End Get
        End Property

        ''' <summary>
        ''' Retorna true se existir o botão de proxima pagina for necessário
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property IsNextPageNecessary As Boolean
            Get
                Return IsLastPageNecessary
            End Get
        End Property

        ''' <summary>
        ''' Total de itens da Lista
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property Total As Integer
            Get
                Return If(_total, -1)
            End Get
        End Property

        Private _total As Integer? = Nothing

        ''' <summary>
        ''' Quantidade de itens por página
        ''' </summary>
        ''' <returns></returns>
        Property PageSize As Integer = 0

        ''' <summary>
        ''' Quantidade de páginas
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property PageCount As Integer
            Get
                If (PageSize > 0) Then
                    Return (Total / PageSize).Ceil()
                Else
                    Return 1
                End If
            End Get
        End Property

        ''' <summary>
        ''' Quantidade média de "botões de paginação" contidas no <see cref="PageRange"/>
        ''' </summary>
        ''' <returns></returns>
        Property PaginationOffset As Integer = 3

        ''' <summary>
        ''' Retorna um range de páginas a partir da pagina atual
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property PageRange As Integer()
            Get
                Dim frange = 1
                Dim lrange = 1
                If PageCount > 1 Then
                    Dim midrange = Math.Ceiling(PaginationOffset / 2)
                    frange = {(PageNumber - midrange), 1}.Max()
                    lrange = {(PageNumber + midrange), PageCount}.Min()
                End If
                Dim arr = New List(Of Integer)
                For index = frange To lrange
                    arr.Add(index)
                Next
                Return arr.ToArray
            End Get
        End Property

        ''' <summary>
        ''' Quantidade de botões de paginação
        ''' </summary>
        ''' <returns></returns>
        Public Function ButtonCount(Optional Trailling = "...") As Integer
            Return CreatePaginationButtons(Trailling).Count()
        End Function

        ''' <summary>
        ''' Verifica se o <see cref="PageRange"/> contém algumas páginas especificas
        ''' </summary>
        ''' <param name="PageNumbers"></param>
        ''' <returns></returns>
        Public Function ContainsPage(PageNumbers As IEnumerable(Of Integer)) As Boolean
            Return PageRange.ContainsAny(PageNumbers)
        End Function

        ''' <summary>
        ''' Verifica se o <see cref="PageRange"/> contém algumas páginas especificas
        ''' </summary>
        ''' <param name="PageNumbers"></param>
        ''' <returns></returns>
        Public Function ContainsPage(ParamArray PageNumbers As Integer()) As Boolean
            Return ContainsPage(If(PageNumbers, {}).AsEnumerable())
        End Function

        ''' <summary>
        ''' Indica se o primeiro botão de reticencias é necessário
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsFirstTraillingNecessary
            Get
                Return IsFirstPageNecessary AndAlso Not ContainsPage(FirstPage, FirstPage + 1)
            End Get
        End Property

        ''' <summary>
        ''' Indica se o ultimo botão de reticencias é necessário
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsLastTraillingNecessary
            Get
                Return IsLastPageNecessary AndAlso Not ContainsPage(LastPage, LastPage - 1)
            End Get
        End Property

        ''' <summary>
        ''' Botões de paginação
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property PageButtons As IEnumerable(Of String)
            Get
                Return CreatePaginationButtons()
            End Get
        End Property

        ''' <summary>
        ''' Cria uma lista de strings utilizadas nos botões de paginação
        ''' </summary>
        ''' <param name="Trailling"></param>
        ''' <returns></returns>
        Public Function CreatePaginationButtons(Optional Trailling As String = "...") As IEnumerable(Of String)
            Dim l As New List(Of String)
            If IsPaginationNecessary Then
                If IsFirstPageNecessary Then
                    l.Add(FirstPage.ToString())
                End If
                If Trailling.IsNotBlank AndAlso IsFirstTraillingNecessary Then
                    l.Add(Trailling)
                End If
                l.AddRange(PageRange.Select(Function(x) x.ToString()))
                If Trailling.IsNotBlank AndAlso IsLastTraillingNecessary Then
                    l.Add(Trailling)
                End If
                If IsLastPageNecessary Then
                    l.Add(LastPage.ToString())
                End If
            End If
            Return l
        End Function

        ''' <summary>
        ''' Aplica a paginação a um template
        ''' </summary>
        ''' <param name="Template">Template de pagina</param>
        ''' <param name="TraillingTemplate">emplate de botoes de reticencias</param>
        ''' <param name="Trailling">botao de reticencias</param>
        ''' <returns></returns>
        Public Function PageButtonsFromTemplate(Template As String, TraillingTemplate As String, Optional SeparatorTemplate As String = "", Optional Trailling As String = "...") As String
            If Template.IsNotBlank Then
                If TraillingTemplate.IsBlank OrElse Trailling.IsBlank Then
                    Return PageButtonsFromTemplate(Template, SeparatorTemplate)
                Else
                    If Trailling.IsNumber() Then Throw New ArgumentException($"Trailling cannot be a number! => {Trailling}")
                    Return CreatePaginationButtons(Trailling).Select(Function(x)

                                                                         If x.IsNumber Then
                                                                             Return Template.Inject(New With {.Page = x})
                                                                         End If
                                                                         If x = Trailling Then
                                                                             Return TraillingTemplate.Inject(New With {.Page = x, .Trailling = Trailling})
                                                                         End If
                                                                         Return ""
                                                                     End Function).Join(SeparatorTemplate.IfBlank(""))

                End If
            End If
            Return ""
        End Function
        ''' <summary>
        ''' Aplica a paginação a um template
        ''' </summary>
        ''' <param name="Template">Template de pagina</param>
        ''' <returns></returns>
        Public Function PageButtonsFromTemplate(Template As String, Optional SeparatorTemplate As String = "") As String
            If Template.IsNotBlank() Then Return CreatePaginationButtons("").Select(Function(x) Template.Inject(New With {.Page = x})).Join(SeparatorTemplate.IfBlank(""))
            Return ""
        End Function


        ''' <summary>
        ''' Configura este Filtro
        ''' </summary>
        ''' <param name="options"></param>
        ''' <returns></returns>
        Function Config(options As Action(Of PaginationFilter(Of ClassType, RemapType))) As PaginationFilter(Of ClassType, RemapType)
            options(Me)
            Return Me
        End Function

        ''' <summary>
        ''' Configura este Filtro para utilizar uma querystring.
        ''' </summary>
        ''' <param name="Query">QueryString</param>
        ''' <remarks> Utiliza os names como propriedade e os values como valores do filtro. Propriedade que não existirem na classe serão ignoradas. Valores nulos serão ignorados por padrão</remarks>
        ''' <returns></returns>
        Function UseQueryString(Query As String, Optional DefaultOperator As String = "=") As PaginationFilter(Of ClassType, RemapType)
            If Query.IsNotBlank Then
                UseNameValueCollection(Query.ParseQueryString, DefaultOperator)
            End If
            Return Me
        End Function

        ''' <summary>
        ''' Configura este Filtro para utilizar uma querystring com operadores (&membro=operador:valor)
        ''' </summary>
        ''' <param name="QueryExpression"></param>
        ''' <returns></returns>
        Function UseQueryStringExpression(QueryExpression As String, Optional Separator As String = ":", Optional Conditional As FilterConditional = FilterConditional.And) As PaginationFilter(Of ClassType, RemapType)
            Dim Collection = QueryExpression.ParseQueryString()
            For Each K In Collection.AllKeys
                Dim prop = K.UrlDecode()
                Dim t = GetType(ClassType)
                If t.HasProperty(prop) OrElse K = "this" Then
                    If Collection(K).IsNotBlank() AndAlso Collection.GetValues(K).Any() Then
                        Dim buscas = Collection.GetValues(K).GroupBy(Function(x) x.GetBefore(Separator, True).IfBlank("=")).ToDictionary()
                        For Each item In buscas
                            Dim vals = item.Value.Select(Function(x) x.GetAfter(Separator))
                            Me.SetMember(prop, Conditional).SetValues(vals).SetOperator(item.Key).QueryStringSeparator = Separator
                        Next
                    End If
                End If
            Next
            Return Me
        End Function

        ''' <summary>
        ''' Configura este LambDafilter para utilizar um Dictionary como Filtro.
        ''' </summary>
        ''' <param name="Collection">Collection</param>
        ''' <remarks> Utiliza os names como propriedade e os values como valores do filtro. Propriedade que não existirem na classe serão ignoradas. Valores nulos serão ignorados por padrão</remarks>
        ''' <returns></returns>
        Function UseArrayDictionary(Collection As IDictionary(Of String, IComparable()), Optional DefaultOperator As String = "=") As PaginationFilter(Of ClassType, RemapType)
            Collection = If(Collection, New Dictionary(Of String, IComparable()))
            For Each K In Collection.Keys
                Dim t = GetType(ClassType)
                If t.HasProperty(K) OrElse K = "this" Then
                    Me.SetMember(K).SetValues(Collection(K).ToArray()).SetOperator(DefaultOperator)
                End If
            Next
            Return Me
        End Function

        ''' <summary>
        ''' Configura este LambDafilter para utilizar um Dictionary como Filtro.
        ''' </summary>
        ''' <param name="Collection">Collection</param>
        ''' <remarks> Utiliza os names como propriedade e os values como valores do filtro. Propriedade que não existirem na classe serão ignoradas. Valores nulos serão ignorados por padrão</remarks>
        ''' <returns></returns>
        Function UseDictionary(Collection As IDictionary(Of String, IComparable), Optional DefaultOperator As String = "=") As PaginationFilter(Of ClassType, RemapType)
            Collection = If(Collection, New Dictionary(Of String, IComparable))
            For Each K In Collection.Keys
                Dim t = GetType(ClassType)
                If t.HasProperty(K) OrElse K = "this" Then
                    Dim item As IComparable = Collection(K)
                    Me.SetMember(K).SetValue(item).SetOperator(DefaultOperator)
                End If
            Next
            Return Me
        End Function

        ''' <summary>
        ''' Seta a lista com os dados a serem filtrados nesse filtro
        ''' </summary>
        ''' <param name="List"></param>
        ''' <returns></returns>
        Public Function SetData(List As IEnumerable(Of ClassType)) As PaginationFilter(Of ClassType, RemapType)
            Me.Data = List
            Return Me
        End Function

        ''' <summary>
        ''' Seta a lista com os dados a serem filtrados nesse filtro
        ''' </summary>
        ''' <param name="List"></param>
        ''' <returns></returns>
        Public Function SetData(List As IQueryable(Of ClassType)) As PaginationFilter(Of ClassType, RemapType)
            Me.Data = List.AsQueryable()
            Return Me
        End Function

        Public Function OrderBy(Of T)(ParamArray Selectors As Expression(Of Func(Of ClassType, T))()) As PaginationFilter(Of ClassType, RemapType)
            For Each Selector In If(Selectors, {})
                If Selector IsNot Nothing Then
                    Me.OrderBy(Selector)
                End If
            Next
            Return Me
        End Function

        Public Function OrderByDescending(Of T)(Selector As Expression(Of Func(Of ClassType, T))) As PaginationFilter(Of ClassType, RemapType)
            If Selector IsNot Nothing Then
                Me.OrderBy(Selector, True)
            End If

            Return Me
        End Function

        ''' <summary>
        ''' Ordena os resultados da lista
        ''' </summary>
        ''' <typeparam name="t"></typeparam>
        ''' <param name="Selector"></param>
        ''' <param name="Descending"></param>
        ''' <returns></returns>
        Public Function OrderBy(Of T)(Selector As Expression(Of Func(Of ClassType, T)), Optional Descending As Boolean = False) As PaginationFilter(Of ClassType, RemapType)
            Dim Ascending = Not Descending
            If Selector IsNot Nothing Then
                If TypeOf Me.Data Is IOrderedQueryable(Of ClassType) Then
                    If Ascending Then
                        Me.Data = CType(Me.Data, IOrderedQueryable(Of ClassType)).ThenBy(Selector)
                    Else
                        Me.Data = CType(Me.Data, IOrderedQueryable(Of ClassType)).ThenByDescending(Selector)
                    End If
                    Return Me
                End If
                If TypeOf Me.Data Is IQueryable(Of ClassType) Then
                    If Ascending Then
                        Me.Data = CType(Me.Data, IQueryable(Of ClassType)).OrderBy(Selector)
                    Else
                        Me.Data = CType(Me.Data, IQueryable(Of ClassType)).OrderByDescending(Selector)
                    End If
                    Return Me
                End If
                If TypeOf Me.Data Is IOrderedEnumerable(Of ClassType) Then
                    If Ascending Then
                        Me.Data = CType(Me.Data, IOrderedEnumerable(Of ClassType)).ThenBy(Selector.Compile)
                    Else
                        Me.Data = CType(Me.Data, IOrderedEnumerable(Of ClassType)).ThenByDescending(Selector.Compile)
                    End If
                    Return Me
                End If
                If TypeOf Me.Data Is IEnumerable(Of ClassType) Then
                    If Ascending Then
                        Me.Data = CType(Me.Data, IEnumerable(Of ClassType)).OrderBy(Selector.Compile)
                    Else
                        Me.Data = CType(Me.Data, IEnumerable(Of ClassType)).OrderByDescending(Selector.Compile)
                    End If
                    Return Me
                End If
            End If
            Return Me
        End Function

        ''' <summary>
        ''' Ordena os resultados da lista
        ''' </summary>
        ''' <returns></returns>
        Public Function OrderBy(Selector As String(), Optional Descending As Boolean = False) As PaginationFilter(Of ClassType, RemapType)
            Dim Ascending = Not Descending
            If If(Selector, {}).Any() Then
                If TypeOf Me.Data Is IQueryable(Of ClassType) Then
                    Me.Data = CType(Me.Data, IQueryable(Of ClassType)).ThenByProperty(Selector, Ascending)
                    Return Me
                End If
                If TypeOf Me.Data Is IEnumerable(Of ClassType) Then
                    Me.Data = CType(Me.Data, IEnumerable(Of ClassType)).ThenByProperty(Selector, Ascending)
                    Return Me
                End If
            End If
            Return Me
        End Function

        ''' <summary>
        ''' Ordena os resultados da lista
        ''' </summary>
        ''' <param name="Selector"></param>
        ''' <param name="Descending"></param>
        ''' <returns></returns>
        Public Function OrderBy(Selector As String, Optional Descending As Boolean = False) As PaginationFilter(Of ClassType, RemapType)
            Return Me.OrderBy(Selector.IfBlank("").SplitAny(" ", "/", ","), Descending)
        End Function

        ''' <summary>
        ''' Extrai os parametros de um <see cref="NameValueCollection"/> e seta os membros usando as Keys como membros
        ''' </summary>
        ''' <param name="Collection"></param>
        ''' <param name="DefaultOperator"></param>
        ''' <returns></returns>
        Public Function UseNameValueCollection(Collection As NameValueCollection, Optional DefaultOperator As String = "=") As PaginationFilter(Of ClassType, RemapType)
            Collection = If(Collection, New NameValueCollection)
            For Each K In Collection.AllKeys
                Dim t = GetType(ClassType)
                Dim l = t.GetProperties()
                If l.Any(Function(x) x.Name = K) Then
                    If Collection(K).IsNotBlank() AndAlso Collection.GetValues(K).Any() Then
                        Me.SetMember(K).SetValues(Collection.GetValues(K)).SetOperator(DefaultOperator)
                    End If
                End If
            Next
            Return Me
        End Function

        ''' <summary>
        ''' Executa o Filtro e retorna os dados paginados
        ''' </summary>
        ''' <param name="PageNumber"></param>
        ''' <returns></returns>
        Public Function GetPage(PageNumber As Integer) As RemapType()
            If Me.Data IsNot Nothing Then
                Dim filtereddata = GetQueryablePage(PageNumber)
                If RemapExpression Is Nothing OrElse GetType(ClassType) Is GetType(RemapType) Then
                    Return filtereddata.Cast(Of RemapType).ToArray()
                End If
                Return filtereddata.Select(RemapExpression).ToArray()
            End If
            Return {}
        End Function

        ''' <summary>
        ''' Expressões adicionadas a clausula where junto com os filtros
        ''' </summary>
        ''' <returns></returns>
        Public Property WhereFilters As New List(Of Expression(Of Func(Of ClassType, Boolean)))

        ''' <summary>
        ''' Adciona Expressões a clausula where junto com os filtros
        ''' </summary>
        ''' <returns></returns>
        Public Function Where(predicate As Expression(Of Func(Of ClassType, Boolean))) As PaginationFilter(Of ClassType, RemapType)
            WhereFilters = If(WhereFilters, New List(Of Expression(Of Func(Of ClassType, Boolean))))
            WhereFilters.Add(predicate)
            Return Me
        End Function

        ''' <summary>
        ''' Adciona Expressões a clausula where junto com os filtros se uma condiçao for cumprida
        ''' </summary>
        ''' <returns></returns>
        Public Function WhereIf(Test As Boolean, predicate As Expression(Of Func(Of ClassType, Boolean))) As PaginationFilter(Of ClassType, RemapType)
            If Test Then Where(predicate)
            Return Me
        End Function

        ''' <summary>
        ''' Retorna a pagina atual
        ''' </summary>
        ''' <returns></returns>
        Public Function GetPage() As RemapType()
            Return GetPage(PageNumber)
        End Function

        ''' <summary>
        ''' Retorna <see cref="Data"/> com os filtros aplicados
        ''' </summary>
        ''' <returns></returns>
        Public Function GetQueryablePage() As IQueryable(Of ClassType)
            Return GetQueryablePage(PageNumber)
        End Function

        ''' <summary>
        ''' Retorna <see cref="Data"/> com os filtros aplicados
        ''' </summary>
        ''' <param name="PageNumber"></param>
        ''' <returns></returns>
        Public Function GetQueryablePage(PageNumber As Integer) As IQueryable(Of ClassType)
            Me.PageNumber = PageNumber
            If Me.Data IsNot Nothing Then
                Dim filtereddata = ApplyFilter()
                filtereddata = ApplyPage(filtereddata)
                Return filtereddata
            End If
            Return Me.Data
        End Function

        ''' <summary>
        ''' Retorna <see cref="Data"/> com os filtros aplicados
        ''' </summary>
        ''' <returns></returns>
        Public Function GetEnumerablePage() As IQueryable(Of ClassType)
            Return GetEnumerablePage(PageNumber)
        End Function

        ''' <summary>
        ''' Retorna <see cref="Data"/> com os filtros aplicados
        ''' </summary>
        ''' <param name="PageNumber"></param>
        ''' <returns></returns>
        Public Function GetEnumerablePage(PageNumber As Integer) As IEnumerable(Of ClassType)
            Return GetQueryablePage(PageNumber).AsEnumerable()
        End Function

        ''' <summary>
        ''' Configura a paginação do filtro
        ''' </summary>
        ''' <param name="PageSize"></param>
        ''' <param name="PaginationOffset"></param>
        ''' <returns></returns>
        Function SetPagination(PageSize As Integer, PaginationOffset As Integer) As PaginationFilter(Of ClassType, RemapType)
            Me.PageSize = PageSize
            Me.PaginationOffset = PaginationOffset
            Return Me
        End Function

        ''' <summary>
        ''' Configura a paginação do filtro
        ''' </summary>
        ''' <param name="PageSize"></param>
        ''' <returns></returns>
        Function SetPagination(PageSize As Integer) As PaginationFilter(Of ClassType, RemapType)
            Me.PageSize = PageSize
            Return Me
        End Function

        ''' <summary>
        ''' Seta a pagina atual
        ''' </summary>
        ''' <param name="PageNumber"></param>
        ''' <returns></returns>
        Function SetPage(PageNumber As Integer) As PaginationFilter(Of ClassType, RemapType)
            Me.PageNumber = PageNumber
            Return Me
        End Function

        Function [And](Of T)(PropertyName As String, Optional Enabled As Boolean = True) As PropertyFilter(Of ClassType, RemapType)
            Return SetMember(PropertyName, FilterConditional.And, Enabled)
        End Function

        Function [Or](Of T)(PropertyName As String, Optional Enabled As Boolean = True) As PropertyFilter(Of ClassType, RemapType)
            Return SetMember(PropertyName, FilterConditional.Or, Enabled)
        End Function

        Function [And](Of T)(PropertyName As Expression(Of Func(Of ClassType, T)), Optional Enabled As Boolean = True) As PropertyFilter(Of ClassType, RemapType)
            Return SetMember(PropertyName, FilterConditional.And, Enabled)
        End Function

        Function [Or](Of T)(PropertyName As Expression(Of Func(Of ClassType, T)), Optional Enabled As Boolean = True) As PropertyFilter(Of ClassType, RemapType)
            Return SetMember(PropertyName, FilterConditional.Or, Enabled)
        End Function

        ''' <summary>
        ''' Configura um novo membro para este filtro
        ''' </summary>
        ''' <param name="PropertyName"></param>
        ''' <returns></returns>
        Function SetMember(Of T)(PropertyName As Expression(Of Func(Of ClassType, T)), Optional Conditional As FilterConditional = FilterConditional.Or, Optional Enabled As Boolean = True) As PropertyFilter(Of ClassType, RemapType)
            Dim f = New PropertyFilter(Of ClassType, RemapType)(Me)
            f.SetMember(PropertyName, Conditional).SetEnabled(Enabled)
            _filters.Add(f)
            Return f
        End Function

        ''' <summary>
        ''' Seta uma busca usando <see cref="Contains()"/> em <paramref name="PropertyValues"/> para cada propriedade em <paramref name="PropertyNames"/>
        ''' </summary>
        ''' <param name="PropertyValues"></param>
        ''' <param name="PropertyNames"></param>
        ''' <returns></returns>
        Function CreateSearch(Of T)(PropertyValues As IEnumerable(Of IComparable), ParamArray PropertyNames As Expression(Of Func(Of ClassType, T))()) As PaginationFilter(Of ClassType, RemapType)
            PropertyNames = If(PropertyNames, {}).Where(Function(x) x IsNot Nothing).ToArray()
            PropertyValues = If(PropertyValues, {})
            For Each sel In PropertyNames
                Me.SetMember(sel, FilterConditional.Or).Contains(PropertyValues)
            Next
            Return Me
        End Function

        ''' <summary>
        ''' Seta uma busca usando <see cref="Contains(<paramref name="PropertyValues"/>)"/> para cada propriedade em <paramref name="PropertyNames"/>
        ''' </summary>
        ''' <param name="PropertyValues"></param>
        ''' <param name="PropertyNames"></param>
        ''' <returns></returns>
        Function CreateSearch(PropertyValues As IEnumerable(Of IComparable), ParamArray PropertyNames As String()) As PaginationFilter(Of ClassType, RemapType)
            PropertyNames = If(PropertyNames, {}).Where(Function(x) x.IsNotBlank()).ToArray()
            PropertyValues = If(PropertyValues, {})
            For Each sel In PropertyNames
                Me.SetMember(sel, FilterConditional.Or).Contains(PropertyValues)
            Next
            Return Me
        End Function

        ''' <summary>
        ''' Configura um novo membro para este filtro
        ''' </summary>
        ''' <param name="PropertyName"></param>
        ''' <returns></returns>
        Function SetMember(PropertyName As String, Optional Conditional As FilterConditional = FilterConditional.Or, Optional Enabled As Boolean = True) As PropertyFilter(Of ClassType, RemapType)
            Dim f = New PropertyFilter(Of ClassType, RemapType)(Me)
            f.SetMember(PropertyName, Conditional).SetEnabled(Enabled)
            _filters.Add(f)
            Return f
        End Function

        ''' <summary>
        ''' Verifica se á pagina atual é igual a uma pagina especifica
        ''' </summary>
        ''' <param name="Index"></param>
        ''' <returns></returns>
        Public Function IsCurrentPage(Index As Integer) As Boolean
            Return Index = PageNumber
        End Function

        Private Function ApplyFilter() As IEnumerable(Of ClassType)
            Dim FilteredData = Me.Data
            _total = Nothing
            If FilteredData IsNot Nothing Then
                If LambdaExpression IsNot Nothing Then
                    If TypeOf FilteredData Is IOrderedQueryable(Of ClassType) Then
                        FilteredData = CType(FilteredData, IOrderedQueryable(Of ClassType)).Where(LambdaExpression)
                        Dim dq = CType(FilteredData, IOrderedQueryable(Of ClassType)).Select(Function(x) 0)
                        _total = dq.Count()
                    End If
                    If TypeOf FilteredData Is IQueryable(Of ClassType) Then
                        FilteredData = CType(FilteredData, IQueryable(Of ClassType)).Where(LambdaExpression)
                        Dim dq = CType(FilteredData, IQueryable(Of ClassType)).Select(Function(x) 0)
                        _total = dq.Count()
                    Else
                        FilteredData = FilteredData.Where(LambdaExpression.Compile())
                        _total = FilteredData.Count()
                    End If
                Else
                    _total = FilteredData.Count()
                End If
                Return FilteredData
            End If
            Return Me.Data
        End Function

        Private Function ApplyPage(FilteredData As IEnumerable(Of ClassType)) As IEnumerable(Of ClassType)
            If Me.Data IsNot Nothing Then
                If PageNumber > 0 AndAlso PageSize > 0 Then
                    If TypeOf FilteredData Is IOrderedQueryable(Of ClassType) Then
                        FilteredData = CType(FilteredData, IOrderedQueryable(Of ClassType)).Skip((PageNumber - 1) * PageSize).Take(PageSize)
                    ElseIf TypeOf Me.Data Is IQueryable(Of ClassType) Then
                        FilteredData = CType(FilteredData, IQueryable(Of ClassType)).Skip((PageNumber - 1) * PageSize).Take(PageSize)
                    End If
                End If
                Return FilteredData
            End If
            Return Me.Data
        End Function

        Public Shared Widening Operator CType(obj As PaginationFilter(Of ClassType, RemapType)) As RemapType()
            Return obj.GetPage()
        End Operator

        Public Shared Widening Operator CType(obj As PaginationFilter(Of ClassType, RemapType)) As List(Of RemapType)
            Return obj.GetPage().ToList()
        End Operator

        Public Shared Widening Operator CType(NVC As NameValueCollection) As PaginationFilter(Of ClassType, RemapType)
            Return New PaginationFilter(Of ClassType, RemapType)().UseNameValueCollection(NVC)
        End Operator

        Public Shared Widening Operator CType(QueryString As String) As PaginationFilter(Of ClassType, RemapType)
            Return New PaginationFilter(Of ClassType, RemapType)().UseQueryStringExpression(QueryString)
        End Operator

    End Class

    Public Class PaginationFilter(Of ClassType As Class)
        Inherits PaginationFilter(Of ClassType, ClassType)

        Sub New()
        End Sub

        ''' <summary>
        ''' Cria uma nova instancia e seta a exclusividade de filtro
        ''' </summary>
        Sub New(Options As Action(Of PaginationFilter(Of ClassType)))
            Options(Me)
        End Sub

    End Class

    Public Enum FilterConditional
        [Or]
        [And]
    End Enum

    Public Class PropertyFilter(Of ClassType As Class, RemapType)

        Friend Sub New(LB As PaginationFilter(Of ClassType, RemapType))
            PaginationFilter = LB
        End Sub

        Public ReadOnly Property PaginationFilter As PaginationFilter(Of ClassType, RemapType)

        ''' <summary>
        ''' Expressão binaria deste filtro
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property Filter As BinaryExpression
            Get
                If Enabled Then
                    Dim v = ValidValues()

                    Return GetOperatorExpression(Member, [Operator].IfBlank(""), v, ValuesConditional)
                End If
                Return Nothing
            End Get
        End Property

        Public Property ValueValidation As Expression(Of Func(Of IComparable, Boolean)) = Nothing

        ''' <summary>
        ''' Retorna apenas os valores validos para este filtro (<see cref="AcceptNullValues"/> e <see cref="ValueValidation"/>)
        ''' </summary>
        ''' <returns></returns>
        Public Function ValidValues() As IEnumerable(Of IComparable)
            Dim v = If(Me.PropertyValues, {}).AsEnumerable()
            If Not AcceptNullValues Then
                v = v.Where(Function(x) x IsNot Nothing)
            End If
            If ValueValidation IsNot Nothing Then
                v = v.Where(ValueValidation.Compile())
            End If
            Return v
        End Function

        Property Conditional As FilterConditional = FilterConditional.Or

        Property ValuesConditional As FilterConditional = FilterConditional.Or

        ''' <summary>
        ''' Configura este filtro para utilização de valores nulos na query
        ''' </summary>
        ''' <returns></returns>
        Property AcceptNullValues As Boolean = False

        ''' <summary>
        ''' Indica se este filtro está ativo
        ''' </summary>
        ''' <returns></returns>
        Public Property Enabled As Boolean = True

        ''' <summary>
        ''' Operador usado nesse filtro
        ''' </summary>
        ''' <returns></returns>
        Public Property [Operator] As String = "="

        ''' <summary>
        ''' Comparara o valor do filtro com TRUE ou FALSE
        ''' </summary>
        ''' <returns></returns>
        Public Property [Is] As Boolean = True

        ''' <summary>
        ''' Valores a serem testados por esse filtro
        ''' </summary>
        ''' <returns></returns>
        Public Property PropertyValues As IEnumerable(Of IComparable)

        ''' <summary>
        ''' Parametro da expressão lambda
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Parameter As ParameterExpression
            Get
                Return PaginationFilter.Parameter
            End Get
        End Property

        ''' <summary>
        ''' Expressão do membro utilizado no filtro
        ''' </summary>
        ''' <returns></returns>
        Property Member As Expression

        ''' <summary>
        ''' Seta varios valores para esse filtro testar. Substitui os valores antigos
        ''' </summary>
        ''' <param name="Values"></param>
        ''' <returns></returns>
        Function SetValues(Of T As IComparable)(ParamArray Values As T()) As PropertyFilter(Of ClassType, RemapType)
            PropertyValues = If(Values, {})
            Return Me
        End Function

        ''' <summary>
        ''' Seta varios valores para esse filtro testar. Substitui os valores antigos
        ''' </summary>
        ''' <param name="Values"></param>
        ''' <returns></returns>
        Function SetValues(Of T As IComparable)(Values As IEnumerable(Of T)) As PropertyFilter(Of ClassType, RemapType)
            PropertyValues = If(Values, {})
            Return Me
        End Function

        ''' <summary>
        ''' Adciona varios valores para esse filtro testar.
        ''' </summary>
        ''' <param name="Values"></param>
        ''' <returns></returns>
        Function AddValues(Of T As Structure)(ParamArray Values As T?()) As PropertyFilter(Of ClassType, RemapType)
            PropertyValues = If(PropertyValues, {}).Union(If(Values, {}))
            Return Me
        End Function

        ''' <summary>
        ''' Seta um unico valor para esse filtro testar. Substitui os antigos
        ''' </summary>
        ''' <param name="Value"></param>
        ''' <returns></returns>
        Function SetValue(Of T As IComparable)(Value As T) As PropertyFilter(Of ClassType, RemapType)
            PropertyValues = {Value}
            Return Me
        End Function

        ''' <summary>
        ''' Seta um unico valor para esse filtro testar. Substitui os antigos
        ''' </summary>
        ''' <param name="Value"></param>
        ''' <returns></returns>
        Function SetValue(Of T As Structure)(Value As T?) As PropertyFilter(Of ClassType, RemapType)
            PropertyValues = {Value}
            Return Me
        End Function

        ''' <summary>
        ''' Sete um membro para ser utilizado neste filtro. É ignorado quando seus Values estão nulos ou vazios
        ''' </summary>
        ''' <param name="PropertySelector"></param>
        ''' <returns></returns>
        Function SetMember(Of T)(PropertySelector As Expression(Of Func(Of ClassType, T)), Optional Conditional As FilterConditional = FilterConditional.Or) As PropertyFilter(Of ClassType, RemapType)
            Return SetMember(PropertySelector.Body.ToString().Split(".").Skip(1).Join("."), Conditional)
        End Function

        ''' <summary>
        ''' Sete um membro para ser utilizado neste filtro. É ignorado quando seus Values estão nulos ou vazios
        ''' </summary>
        ''' <param name="PropertyName"></param>
        ''' <returns></returns>
        Function SetMember(PropertyName As String, Optional Conditional As FilterConditional = FilterConditional.Or) As PropertyFilter(Of ClassType, RemapType)
            Me.Conditional = Conditional
            Member = Parameter.PropertyExpression(PropertyName)
            Return Me
        End Function

        ''' <summary>
        ''' Seta o operador utilizado nesse filtro
        ''' </summary>
        ''' <param name="[Operator]"></param>
        ''' <returns></returns>
        Function SetOperator([Operator] As String) As PropertyFilter(Of ClassType, RemapType)
            Me.Operator = [Operator].IfBlank("=").ToLower
            Return Me
        End Function

        Public ReadOnly Property CompareWith As Boolean
            Get
                Return Not [Operator].StartsWithAny("!")
            End Get
        End Property

        ''' <summary>
        ''' Nega o filtro atual
        ''' </summary>
        ''' <returns></returns>
        Function Negate() As PropertyFilter(Of ClassType, RemapType)
            If CompareWith = False Then
                [Operator] = [Operator].RemoveFirstAny(False, "!")
            Else
                [Operator] = "!" & [Operator]
            End If
            Return Me
        End Function

        ''' <summary>
        ''' Força uma comparação positiva para este filtro
        ''' </summary>
        ''' <returns></returns>
        Function CompareTrue() As PropertyFilter(Of ClassType, RemapType)
            If CompareWith = False Then
                Me.Negate()
            End If
            Return Me
        End Function

        ''' <summary>
        ''' Força uma comparação negativa para este filtro
        ''' </summary>
        ''' <returns></returns>
        Function CompareFalse() As PropertyFilter(Of ClassType, RemapType)
            If CompareWith = True Then
                Me.Negate()
            End If
            Return Me
        End Function

        ''' <summary>
        ''' Seta o operador para Contains e o Valor para este filtro
        ''' </summary>
        ''' <returns></returns>
        Function ContainsAll(Of T As Structure)(Value As T?) As PaginationFilter(Of ClassType, RemapType)
            Me.SetValue(Value)
            Me.SetOperator("contains")
            Me.ValuesConditional = FilterConditional.And
            Return Me.PaginationFilter
        End Function

        ''' <summary>
        ''' Seta o operador para Contains e os Valores para este filtro
        ''' </summary>
        ''' <returns></returns>
        Function ContainsAll(Of T As IComparable)(Values As IEnumerable(Of T)) As PaginationFilter(Of ClassType, RemapType)
            Me.SetValues(Values)
            Me.SetOperator("contains")
            Me.ValuesConditional = FilterConditional.And
            Return Me.PaginationFilter
        End Function

        ''' <summary>
        ''' Seta o operador para Contains e o Valor para este filtro
        ''' </summary>
        ''' <returns></returns>
        Function Contains(Of T As Structure)(Value As T?) As PaginationFilter(Of ClassType, RemapType)
            Me.SetValue(Value)
            Me.SetOperator("contains")
            Return Me.PaginationFilter
        End Function

        ''' <summary>
        ''' Seta o operador para Contains e os Valores para este filtro
        ''' </summary>
        ''' <returns></returns>
        Function Contains(Of T As IComparable)(Values As IEnumerable(Of T)) As PaginationFilter(Of ClassType, RemapType)
            Me.SetValues(Values)
            Me.SetOperator("contains")
            Return Me.PaginationFilter
        End Function

        ''' <summary>
        ''' Seta o operador para StartsWith e o Valor para este filtro
        ''' </summary>
        ''' <returns></returns>
        Function StartsWith(Of T As Structure)(Value As T?) As PaginationFilter(Of ClassType, RemapType)
            Me.SetValue(Value)
            Me.SetOperator("StartsWith")
            Return Me.PaginationFilter
        End Function

        ''' <summary>
        ''' Seta o operador para StartsWith e os Valores para este filtro
        ''' </summary>
        ''' <returns></returns>
        Function StartsWith(Of T As IComparable)(Values As IEnumerable(Of T)) As PaginationFilter(Of ClassType, RemapType)
            Me.SetValues(Values)
            Me.SetOperator("StartsWith")
            Return Me.PaginationFilter
        End Function

        ''' <summary>
        ''' Seta o operador para EndsWith e o Valor para este filtro
        ''' </summary>
        ''' <returns></returns>
        Function EndsWith(Of T As Structure)(Value As T?) As PaginationFilter(Of ClassType, RemapType)
            Me.SetValue(Value)
            Me.SetOperator("EndsWith")
            Return Me.PaginationFilter
        End Function

        ''' <summary>
        ''' Seta o operador para EndsWith e os Valores para este filtro
        ''' </summary>
        ''' <returns></returns>
        Function EndsWith(Of T As IComparable)(Values As IEnumerable(Of T)) As PaginationFilter(Of ClassType, RemapType)
            Me.SetValues(Values.Cast(Of IComparable))
            Me.SetOperator("EndsWith")
            Return Me.PaginationFilter
        End Function

        ''' <summary>
        ''' Seta o operador para CrossContains e o Valor para este filtro
        ''' </summary>
        ''' <returns></returns>
        Function CrossContains(Of T As Structure)(Value As T?) As PaginationFilter(Of ClassType, RemapType)
            Me.SetValue(Value)
            Me.SetOperator("crosscontains")
            Return Me.PaginationFilter
        End Function

        ''' <summary>
        ''' Seta o operador para CrossContains e os Valores para este filtro
        ''' </summary>
        ''' <returns></returns>
        Function CrossContains(Of T As IComparable)(Values As IEnumerable(Of T)) As PaginationFilter(Of ClassType, RemapType)
            Me.SetValues(Values.Cast(Of IComparable))
            Me.SetOperator("crosscontains")
            Return Me.PaginationFilter
        End Function

        ''' <summary>
        ''' Seta o operador para = e o Valor para este filtro
        ''' </summary>
        ''' <returns></returns>
        Function Equal(Of T As Structure)(Value As T?) As PaginationFilter(Of ClassType, RemapType)
            Me.SetValue(Value)
            Me.SetOperator("=")
            Return Me.PaginationFilter
        End Function

        ''' <summary>
        ''' Seta o operador para = e os Valores para este filtro
        ''' </summary>
        ''' <returns></returns>
        Function Equal(Of T As IComparable)(Values As IEnumerable(Of T)) As PaginationFilter(Of ClassType, RemapType)
            Me.SetValues(Values.Cast(Of IComparable))
            Me.SetOperator("=")
            Return Me.PaginationFilter
        End Function

        ''' <summary>
        ''' Seta o operador para > e o Valor para este filtro
        ''' </summary>
        ''' <returns></returns>
        Function GreaterThan(Of T As Structure)(Value As T?) As PaginationFilter(Of ClassType, RemapType)
            Me.SetValue(Value)
            Me.SetOperator(">")
            Return Me.PaginationFilter
        End Function

        ''' <summary>
        ''' Seta o operador para > e os Valores para este filtro
        ''' </summary>
        ''' <returns></returns>
        Function GreaterThan(Of T As Structure)(Values As IEnumerable(Of T)) As PaginationFilter(Of ClassType, RemapType)
            Me.SetValues(Values.Cast(Of IComparable))
            Me.SetOperator(">")
            Return Me.PaginationFilter
        End Function

        ''' <summary>
        ''' Seta o operador para &lt; e o Valor para este filtro
        ''' </summary>
        ''' <returns></returns>
        Function LessThan(Of T As Structure)(Value As T?) As PaginationFilter(Of ClassType, RemapType)
            Me.SetValue(Value)
            Me.SetOperator("<")
            Return Me.PaginationFilter
        End Function

        ''' <summary>
        ''' Seta o operador para   &lt; e os Valores para este filtro
        ''' </summary>
        ''' <returns></returns>
        Function LessThan(Of T As IComparable)(Values As IEnumerable(Of T)) As PaginationFilter(Of ClassType, RemapType)
            Me.SetValues(Values.Cast(Of IComparable))
            Me.SetOperator("<")
            Return Me.PaginationFilter
        End Function

        ''' <summary>
        ''' Seta o operador para  >= e o Valor para este filtro
        ''' </summary>
        ''' <returns></returns>
        Function GreaterThanOrEqual(Of T As Structure)(Value As T?) As PaginationFilter(Of ClassType, RemapType)
            Me.SetValue(Value)
            Me.SetOperator(">=")
            Return Me.PaginationFilter
        End Function

        ''' <summary>
        ''' Seta o operador para  >= e os Valores para este filtro
        ''' </summary>
        ''' <returns></returns>
        Function GreaterThanOrEqual(Of T As IComparable)(Values As IEnumerable(Of T)) As PaginationFilter(Of ClassType, RemapType)
            Me.SetValues(Values.Cast(Of IComparable))
            Me.SetOperator(">=")
            Return Me.PaginationFilter
        End Function

        ''' <summary>
        ''' Seta o operador para   &lt;= e o Valor para este filtro
        ''' </summary>
        ''' <returns></returns>
        Function LessThanOrEqual(Of T As Structure)(Value As T?) As PaginationFilter(Of ClassType, RemapType)
            Me.SetValue(Value)
            Me.SetOperator("<=")
            Return Me.PaginationFilter
        End Function

        ''' <summary>
        ''' Seta o operador para   &lt; e os Valores para este filtro
        ''' </summary>
        ''' <returns></returns>
        Function LessThanOrEqual(Of T As IComparable)(Values As IEnumerable(Of T)) As PaginationFilter(Of ClassType, RemapType)
            Me.SetValues(Values.Cast(Of IComparable))
            Me.SetOperator("<=")
            Return Me.PaginationFilter
        End Function

        ''' <summary>
        ''' Seta o operador para  != e o Valor para este filtro
        ''' </summary>
        ''' <returns></returns>
        Function NotEqual(Of T As Structure)(Value As T?) As PaginationFilter(Of ClassType, RemapType)
            Me.SetValue(Value)
            Me.SetOperator("<>")
            Return Me.PaginationFilter
        End Function

        ''' <summary>
        ''' Seta o operador para  != e os Valores para este filtro
        ''' </summary>
        ''' <returns></returns>
        Function NotEqual(Of T As IComparable)(Values As IEnumerable(Of T)) As PaginationFilter(Of ClassType, RemapType)
            Me.SetValues(Values.Cast(Of IComparable))
            Me.SetOperator("<>")
            Return Me.PaginationFilter
        End Function

        ''' <summary>
        ''' Permite que valores nulos sejam adcionados ao filtro
        ''' </summary>
        ''' <returns></returns>
        Function AllowNull() As PropertyFilter(Of ClassType, RemapType)
            AcceptNullValues = True
            Return Me
        End Function

        ''' <summary>
        ''' Impede que valores nulos sejam adcionados ao filtro
        ''' </summary>
        ''' <returns></returns>
        Function IgnoreNull() As PropertyFilter(Of ClassType, RemapType)
            AcceptNullValues = False
            Return Me
        End Function

        ''' <summary>
        ''' Ativa ou desativa esse filtro durante a construção da expressão
        ''' </summary>
        ''' <param name="Enabled"></param>
        ''' <returns></returns>
        Function SetEnabled(Optional Enabled As Boolean = True) As PropertyFilter(Of ClassType, RemapType)
            Me.Enabled = Enabled
            Return Me
        End Function

        Public ReadOnly Property PropertyName As String
            Get
                Return Me.Member.ToString().GetAfter(".")
            End Get
        End Property

        ''' <summary>
        ''' Separador utilizado pelo <see cref="CreateQueryParameter(Boolean)"/>
        ''' </summary>
        ''' <returns></returns>
        Public Property QueryStringSeparator As String = ":"

        ''' <summary>
        ''' Retorna uma string em formato de parametro de QueryString deste filtro
        ''' </summary>
        ''' <param name="ForceEnabled"></param>
        ''' <returns></returns>
        Public Function CreateQueryParameter(Optional ForceEnabled As Boolean = False, Optional OnlyValid As Boolean = True) As String
            If Enabled OrElse ForceEnabled Then
                Dim xx = [Operator].AppendIf(QueryStringSeparator, QueryStringSeparator.IsNotBlank() AndAlso [Operator].ToLower().IsNotAny("", "=", "==", "===")).UrlEncode()
                Return If(OnlyValid, ValidValues(), PropertyValues).Where(Function(x) x IsNot Nothing AndAlso x.ToString().IsNotBlank()).SelectJoin(Function(x) $"{PropertyName}={xx}{x.ToString().UrlEncode()}")
            End If
            Return ""
        End Function

        Public Overrides Function ToString() As String
            Return Me.CreateQueryParameter()
        End Function

    End Class

End Namespace