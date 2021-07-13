﻿Imports System.Net
Imports System.Xml

Namespace Locations

    ''' <summary>
    ''' Representa um deteminado local com suas Informações
    ''' </summary>
    ''' <remarks></remarks>
    Public Class AddressInfo

        ''' <summary>
        ''' Cria um novo objeto de localização vazio
        ''' </summary>
        Public Sub New()
        End Sub

        ''' <summary>
        ''' Cria um objeto de localização e imadiatamente pesquisa as informações de um local através do CEP usando as APIs ViaCEP
        ''' </summary>
        ''' <param name="PostalCode"></param>
        ''' <param name="Number">Numero da casa</param>
        Public Sub New(PostalCode As String, Optional Number As String = Nothing, Optional Complement As String = Nothing)
            Me.PostalCode = PostalCode
            If Number.IsNotBlank() Then Me.Number = Number
            If Complement.IsNotBlank() Then Me.Complement = Complement
            Me.GetInfoByPostalCode()
        End Sub

        ''' <summary>
        ''' Tipo do Endereço
        ''' </summary>
        ''' <returns></returns>
        Property StreetType As String

        ''' <summary>
        ''' O nome do endereço
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        Property StreetName As String

        ''' <summary>
        ''' DDD do local
        ''' </summary>
        ''' <returns></returns>
        Public Property DDD As String

        Public Property IBGE As String
        Public Property GIA As String
        Public Property SIAFI As String

        ''' <summary>
        ''' Logradouro
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property Street As String
            Get
                Return StreetType & " " & StreetName
            End Get
        End Property

        ''' <summary>
        ''' Numero da casa, predio etc.
        ''' </summary>
        ''' <value></value>
        ''' <returns>Numero</returns>

        Property Number As String
        ''' <summary>
        ''' Complemento
        ''' </summary>
        ''' <value></value>
        ''' <returns>Complemento</returns>

        Property Complement As String
        ''' <summary>
        ''' Bairro
        ''' </summary>
        ''' <value></value>
        ''' <returns>Bairro</returns>

        Property Neighborhood As String
        ''' <summary>
        ''' CEP - Codigo de Endereçamento Postal
        ''' </summary>
        ''' <value></value>
        ''' <returns>CEP</returns>

        Property PostalCode As String
            Get
                Return _cep
            End Get
            Set(value As String)
                _cep = AddressInfo.FormatPostalCode(value)
            End Set
        End Property

        ''' <summary>
        ''' Formata uma string de CEP
        ''' </summary>
        ''' <param name="CEP"></param>
        ''' <returns></returns>
        Public Shared Function FormatPostalCode(CEP As String) As String
            CEP = CEP.IfBlank("").Trim()
            If CEP.IsValidCEP Then
                If CEP.IsNumber() Then
                    CEP = CEP.Insert(5, "-")
                End If
                Return CEP
            End If
            Return Nothing
        End Function

        Private _cep As String

        ''' <summary>
        ''' CEP - Codigo de Endereçamento Postal. Alias de <see cref="PostalCode"/>
        ''' </summary>
        ''' <value></value>
        ''' <returns>CEP</returns>
        Property ZipCode As String
            Get
                Return PostalCode
            End Get
            Set(value As String)
                PostalCode = value
            End Set
        End Property

        ''' <summary>
        ''' Cidade
        ''' </summary>
        ''' <value></value>
        ''' <returns>Cidade</returns>

        Property City As String
        ''' <summary>
        ''' Estado
        ''' </summary>
        ''' <value></value>
        ''' <returns>Estado</returns>

        Property State As String
        ''' <summary>
        ''' Unidade federativa
        ''' </summary>
        ''' <value></value>
        ''' <returns>Sigla do estado</returns>

        Property StateCode As String

        ''' <summary>
        ''' País
        ''' </summary>
        ''' <value></value>
        ''' <returns>País</returns>

        Property Country As String

        ''' <summary>
        ''' Coordenada geográfica LATITUDE
        ''' </summary>
        ''' <value></value>
        ''' <returns>Latitude</returns>
        Property Latitude As Decimal

        ''' <summary>
        ''' Coordenada geográfica LONGITUDE
        ''' </summary>
        ''' <value></value>
        ''' <returns>Longitude</returns>
        Property Longitude As Decimal

        ''' <summary>
        ''' Retorna o endereço completo
        ''' </summary>
        ''' <returns>Uma String com o endereço completo devidamente formatado</returns>
        ReadOnly Property Address As String
            Get
                Return ToString(AddressPart.FullAddress)
            End Get
        End Property

        ''' <summary>
        ''' Retorna uma string com as partes dos endereço especificas
        ''' </summary>
        ''' <param name="Parts"></param>
        ''' <returns></returns>
        Public Overloads Function ToString(Parts As IEnumerable(Of AddressPart)) As String
            Parts = If(Parts, {})
            If Parts.Any Then
                Dim d As AddressPart = Parts.First()
                For Each p In Parts.Skip(1)
                    d = d Or p
                Next
                Return ToString(d)
            End If
            Return ToString()
        End Function

        ''' <summary>
        ''' Retorna uma string com as partes dos endereço especificas
        ''' </summary>
        ''' <param name="Parts"></param>
        ''' <returns></returns>
        Public Overloads Function ToString(ParamArray Parts As AddressPart()) As String
            Return ToString(Parts.AsEnumerable())
        End Function


        ''' <summary>
        ''' Retorna uma string com as partes dos endereço especificas 
        ''' </summary>
        ''' <param name="Parts"></param>
        ''' <returns></returns>
        Public Overloads Function ToString(Parts As AddressPart) As String

            ParseType()
            Dim retorno As String = ""

            If StreetType.IsNotBlank() AndAlso ContainsPart(Parts, AddressPart.StreetType) Then retorno &= StreetType
            If StreetName.IsNotBlank() AndAlso ContainsPart(Parts, AddressPart.StreetName) Then retorno &= " " & StreetName
            If Number.IsNotBlank AndAlso ContainsPart(Parts, AddressPart.Number) Then retorno &= (", " & Number)
            If Complement.IsNotBlank AndAlso ContainsPart(Parts, AddressPart.Complement) Then retorno &= (", " & Complement)
            If Neighborhood.IsNotBlank AndAlso ContainsPart(Parts, AddressPart.Neighborhood) Then retorno &= (" - " & Neighborhood)
            If City.IsNotBlank AndAlso ContainsPart(Parts, AddressPart.City) Then retorno &= (" - " & City)

            If ContainsPart(Parts, AddressPart.StateCode) AndAlso StateCode.IsNotBlank Then
                retorno &= (" - " & StateCode)
            Else
                If ContainsPart(Parts, AddressPart.State) AndAlso State.IsNotBlank Then retorno &= (" - " & State)
            End If

            If PostalCode.IsNotBlank AndAlso ContainsPart(Parts, AddressPart.PostalCode) Then retorno &= (" - " & PostalCode)
            If Country.IsNotBlank AndAlso ContainsPart(Parts, AddressPart.Country) Then retorno &= (" - " & Country)
            Return New StructuredText(retorno).ToString().AdjustBlankSpaces().TrimAny(True, ".", " ", ",", " ", "-", " ")
        End Function

        Friend Shared Function ContainsPart(Parts As AddressPart, OtherPart As AddressPart) As Boolean
            Return ((Parts) And OtherPart) <> 0
        End Function

        Friend Sub ParseType()
            If Me.StreetType.IsBlank() Then
                If Me.StreetName.IsNotBlank Then
                    Me.StreetType = AddressTypes.GetAddressType(Me.StreetName)
                    If Me.StreetType.IsNotBlank Then
                        Me.StreetName = New StructuredText(Me.StreetName).ToString.TrimAny(False, AddressTypes.GetAddressTypeList(Me.StreetName)).AdjustBlankSpaces().ToTitle(True).TrimAny(True, " ", ".", " ", ",", " ", "-", " ")
                    End If
                End If
            End If
        End Sub

        ''' <summary>
        ''' Cria uma localização a partir de partes de endereço
        ''' </summary>
        ''' <param name="Address"></param>
        ''' <param name="Number"></param>
        ''' <param name="Complement"></param>
        ''' <param name="Neighborhood"></param>
        ''' <param name="City"></param>
        ''' <param name="State"></param>
        ''' <param name="Country"></param>
        ''' <param name="PostalCode"></param>
        ''' <returns></returns>
        Public Shared Function CreateLocation(Address As String, Optional Number As String = "", Optional Complement As String = "", Optional Neighborhood As String = "", Optional City As String = "", Optional State As String = "", Optional Country As String = "", Optional PostalCode As String = "") As AddressInfo
            Dim l = New AddressInfo()

            If Number.IsBlank Then
                Dim maybe_number = Address.GetAfter(",").GetBefore("-").GetBefore(",").TrimAny(True, " ", ".", " ", ",", " ", "-", " ")
                If maybe_number.Contains(" ") Then
                    Dim parts = maybe_number.Split(" ")
                    maybe_number = parts.FirstOrDefault(Function(x) x.IsNumber())
                    If Complement.IsBlank Then
                        Complement = parts.Where(Function(x) x <> maybe_number).Join(" ")
                    End If
                End If
                Number = maybe_number
            End If

            State = State.AdjustBlankSpaces()

            l.StreetName = Address.RemoveAny(Number.IfBlank("")).ToLower().ToTitle().TrimAny(True, " ", ".", " ", ",", " ", "-", " ").NullIf(Function(x) x.IsBlank())
            l.Neighborhood = Neighborhood.AdjustBlankSpaces().ToLower().ToTitle().NullIf(Function(x) x.IsBlank())
            l.Complement = Complement.AdjustBlankSpaces().ToLower().ToTitle().NullIf(Function(x) x.IsBlank())

            l.Number = Number.NullIf(Function(x) x.IsBlank())
            l.City = City.AdjustBlankSpaces().ToLower().ToTitle().NullIf(Function(x) x.IsBlank())
            If State.Length = 2 Then
                l.StateCode = State.AdjustBlankSpaces().ToUpper().NullIf(Function(x) x.IsBlank())
            Else
                l.State = State.AdjustBlankSpaces().ToLower().ToTitle().NullIf(Function(x) x.IsBlank())
            End If
            l.Country = Country.AdjustBlankSpaces().ToLower().ToTitle().NullIf(Function(x) x.IsBlank())
            l.PostalCode = PostalCode.AdjustBlankSpaces().NullIf(Function(x) x.IsBlank())
            l.ParseType()
            Return l
        End Function

        ''' <summary>
        ''' Retorna uma string de endereço a partir de varias partes de endereco
        ''' </summary>
        ''' <param name="Address"></param>
        ''' <param name="Number"></param>
        ''' <param name="Complement"></param>
        ''' <param name="Neighborhood"></param>
        ''' <param name="City"></param>
        ''' <param name="State"></param>
        ''' <param name="Country"></param>
        ''' <param name="PostalCode"></param>
        ''' <returns></returns>
        Public Shared Function FormatAddress(Address As String, Optional Number As String = "", Optional Complement As String = "", Optional Neighborhood As String = "", Optional City As String = "", Optional State As String = "", Optional Country As String = "", Optional PostalCode As String = "") As String
            Return CreateLocation(Address, Number, Complement, Neighborhood, City, State, Country, PostalCode).Address
        End Function

        ''' <summary>
        ''' Retorna uma String contendo as informações do Local
        ''' </summary>
        ''' <returns>string</returns>
        Public Overrides Function ToString() As String
            Return Address
        End Function

        ''' <summary>
        ''' Retorna as coordenadas geográficas do Local
        ''' </summary>
        ''' <returns>Uma String contendo LATITUDE e LONGITUDE separados por virgula</returns>

        Public Function LatitudeLongitude() As String
            Return Latitude & "," & Longitude
        End Function

        ''' <summary>
        ''' Retorna o endereço de acordo com o CEP contidos em uma variavel do tipo InnerLibs.Location usando a API https://viacep.com.br/
        ''' </summary>
        Public Function GetInfoByPostalCode() As Boolean
            Try
                Dim url = "https://viacep.com.br/ws/" & Me.PostalCode.RemoveAny("-") & "/xml/"
                Using c = New WebClient()
                    Dim x = New XmlDocument()
                    x.LoadXml(c.DownloadString(url))
                    Dim cep = x("xmlcep")
                    Me.Neighborhood = cep("bairro")?.InnerText
                    Me.City = cep("localidade")?.InnerText
                    Me.StateCode = cep("uf")?.InnerText
                    Me.State = Brasil.GetNameOf(Me.StateCode)
                    Me.StreetName = cep("logradouro")?.InnerText
                    Try
                        Me.DDD = cep("ddd")?.InnerText
                    Catch ex As Exception

                    End Try
                    Try
                        Me.IBGE = cep("ibge")?.InnerText
                    Catch ex As Exception

                    End Try
                    Try
                        Me.GIA = cep("gia")?.InnerText
                    Catch ex As Exception

                    End Try
                    Try
                        Me.SIAFI = cep("SIAFI")?.InnerText
                    Catch ex As Exception

                    End Try
                    Me.Country = "Brasil"
                    ParseType()
                End Using
                Return True
            Catch ex As Exception
                Return False
            End Try
        End Function

    End Class

    ''' <summary>
    ''' Partes de um Endereço
    ''' </summary>
    <Flags>
    Public Enum AddressPart
        ''' <summary>
        ''' Tipo do Lograoduro
        ''' </summary>
        StreetType = 1
        ''' <summary>
        ''' Nome do Logradouro
        ''' </summary>
        StreetName = 2
        ''' <summary>
        ''' Logradouro
        ''' </summary>
        Street = StreetType + StreetName
        ''' <summary>
        ''' Numero do local
        ''' </summary>
        Number = 4
        ''' <summary>
        ''' Complemento do local
        ''' </summary>
        Complement = 8
        ''' <summary>
        ''' Numero e complemento
        ''' </summary>
        LocationInfo = Number + Complement

        ''' <summary>
        ''' Logradouro, Numero e complemento
        ''' </summary>
        FullLocationInfo = Street + Number + Complement
        ''' <summary>
        ''' Bairro
        ''' </summary>
        Neighborhood = 16
        ''' <summary>
        ''' Cidade
        ''' </summary>
        City = 32
        ''' <summary>
        ''' Estado
        ''' </summary>
        State = 64
        ''' <summary>
        ''' Cidade e Estado
        ''' </summary>
        CityState = City + State
        ''' <summary>
        ''' UF
        ''' </summary>
        StateCode = 128
        ''' <summary>
        ''' Cidade e UF
        ''' </summary>
        CityStateCode = City + StateCode
        ''' <summary>
        ''' País
        ''' </summary>
        Country = 256
        ''' <summary>
        ''' CEP
        ''' </summary>
        PostalCode = 512

        ''' <summary>
        ''' Endereço completo
        ''' </summary>
        FullAddress = Street + LocationInfo + Neighborhood + CityStateCode + Country + PostalCode

    End Enum

    Public Class AddressTypes

        Public Shared Function GetAddressType(Endereco As String) As String
            Dim tp = Endereco.Split(WordSplitters.ToArray, StringSplitOptions.RemoveEmptyEntries).FirstOr("")
            If tp.IsNotBlank Then
                Dim df = New AddressTypes()
                Return df.GetProperties().FirstOrDefault(Function(x) tp.IsIn(CType(x.GetValue(df), String())) OrElse x.Name = tp)?.Name.IfBlank("")
            End If
            Return ""
        End Function

        Public Shared Function GetAddressTypeList(Endereco As String) As String()
            Dim tp = Endereco.Split(WordSplitters.ToArray, StringSplitOptions.RemoveEmptyEntries).FirstOr("")
            If tp.IsNotBlank Then
                Dim df = New AddressTypes()
                Return df.GetProperties().FirstOrDefault(Function(x) tp.IsIn(CType(x.GetValue(df), String())) OrElse x.Name = tp)?.GetValue(df)
            End If
            Return {}
        End Function

        Public ReadOnly Property Aeroporto As String() = {"Aeroporto", "Ar", "Aero"}
        Public ReadOnly Property Alameda As String() = {"Alameda", "Al", "Alm"}
        Public ReadOnly Property Área As String() = {"Área", "Area"}
        Public ReadOnly Property Avenida As String() = {"Avenida", "Av", "Avn"}
        Public ReadOnly Property Campo As String() = {"Cam", "Camp", "Campo"}
        Public ReadOnly Property Chácara As String() = {"Cha", "Chac", "Chacara"}
        Public ReadOnly Property Colônia As String() = {"Col", "Colonia"}
        Public ReadOnly Property Condomínio As String() = {"Condominio", "Cond"}
        Public ReadOnly Property Comunidade As String() = {"Com", "Comunidade"}
        Public ReadOnly Property Conjunto As String() = {"Conjunto", "Con"}
        Public ReadOnly Property Distrito As String() = {"Dis", "Dst", "Distrito"}
        Public ReadOnly Property Esplanada As String() = {"Esp", "Esplanada"}
        Public ReadOnly Property Estação As String() = {"Estacao", "Est", "st"}
        Public ReadOnly Property Estrada As String() = {"Et", "Es", "Estrada"}
        Public ReadOnly Property Favela As String() = {"Fav", "Favela"}
        Public ReadOnly Property Feira As String() = {"Fei", "Fr", "Feira"}
        Public ReadOnly Property Jardim As String() = {"Jd", "Jardim", "Jar"}
        Public ReadOnly Property Ladeira As String() = {"Lad", "Ld", "Ladeira"}
        Public ReadOnly Property Lago As String() = {"Lago", "Lg"}
        Public ReadOnly Property Lagoa As String() = {"Lagoa", "Lga"}
        Public ReadOnly Property Largo As String() = {"Lrg", "Largo", "Lgo"}
        Public ReadOnly Property Loteamento As String() = {"Lote", "Loteamento", "Lt"}
        Public ReadOnly Property Morro As String() = {"Mr", "Mrr", "Morro", "Mor"}
        Public ReadOnly Property Núcleo As String() = {"Nc", "Nucleo", "Nuc"}
        Public ReadOnly Property Parque As String() = {"Parque", "Pq", "Pk", "Par", "Parq", "Park"}
        Public ReadOnly Property Passarela As String() = {"Pass", "Pas", "Pa", "Passarela"}
        Public ReadOnly Property Pátio As String() = {"Pt", "Pat", "Pateo", "Patio"}
        Public ReadOnly Property Praça As String() = {"Praça", "Pc", "Praca", "Pç"}
        Public ReadOnly Property Quadra As String() = {"Qd", "Quadra", "Quad"}
        Public ReadOnly Property Recanto As String() = {"Rec", "Recanto", "Rc"}
        Public ReadOnly Property Residencial As String() = {"Residencial", "Residencia", "Res", "Resid"}
        Public ReadOnly Property Rodovia As String() = {"Rodovia", "Rod"}
        Public ReadOnly Property Rua As String() = {"Rua", "R"}
        Public ReadOnly Property Setor As String() = {"Setor", "Str"}
        Public ReadOnly Property Sítio As String() = {"Sitio", "Sit"}
        Public ReadOnly Property Travessa As String() = {"Trv", "Travessa", "Tvs"}
        Public ReadOnly Property Trecho As String() = {"Trc", "Trecho"}
        Public ReadOnly Property Trevo As String() = {"Trevo"}
        Public ReadOnly Property Vale As String() = {"Vale", "Val"}
        Public ReadOnly Property Vereda As String() = {"Vereda"}
        Public ReadOnly Property Via As String() = {"Via"}
        Public ReadOnly Property Viaduto As String() = {"Vd", "Viaduto"}
        Public ReadOnly Property Viela As String() = {"Viela"}
        Public ReadOnly Property Vila As String() = {"Vila", "Vl"}
    End Class

End Namespace