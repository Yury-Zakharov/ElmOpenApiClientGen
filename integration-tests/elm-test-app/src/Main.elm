module Main exposing (main)

import Browser
import Html exposing (Html, div, h1, h2, h3, text, button, ul, li, pre, code)
import Html.Attributes exposing (style)
import Html.Events exposing (onClick)
import Http
import Json.Decode as Decode

-- Import generated API modules (will be added after code generation)
-- These imports will be uncommented during integration testing
-- import Api.Users
-- import Api.Posts
-- import Api.Types

-- MODEL

type alias Model =
    { users : List String
    , posts : List String
    , health : Maybe String
    , error : Maybe String
    , loading : Bool
    }

init : () -> (Model, Cmd Msg)
init _ =
    ( { users = []
      , posts = []
      , health = Nothing
      , error = Nothing
      , loading = False
      }
    , Cmd.none
    )

-- UPDATE

type Msg
    = TestGeneratedCode
    | LoadUsers
    | LoadPosts
    | LoadHealth
    | UserResult (Result Http.Error String)
    | PostResult (Result Http.Error String)
    | HealthResult (Result Http.Error String)

update : Msg -> Model -> (Model, Cmd Msg)
update msg model =
    case msg of
        TestGeneratedCode ->
            ( { model | loading = True, error = Nothing }
            , Cmd.batch
                [ loadUsers
                , loadPosts
                , loadHealth
                ]
            )

        LoadUsers ->
            ( { model | loading = True, error = Nothing }
            , loadUsers
            )

        LoadPosts ->
            ( { model | loading = True, error = Nothing }
            , loadPosts
            )

        LoadHealth ->
            ( { model | loading = True, error = Nothing }
            , loadHealth
            )

        UserResult (Ok users) ->
            ( { model | users = [users], loading = False }
            , Cmd.none
            )

        UserResult (Err error) ->
            ( { model | error = Just (httpErrorToString error), loading = False }
            , Cmd.none
            )

        PostResult (Ok posts) ->
            ( { model | posts = [posts], loading = False }
            , Cmd.none
            )

        PostResult (Err error) ->
            ( { model | error = Just (httpErrorToString error), loading = False }
            , Cmd.none
            )

        HealthResult (Ok health) ->
            ( { model | health = Just health, loading = False }
            , Cmd.none
            )

        HealthResult (Err error) ->
            ( { model | error = Just (httpErrorToString error), loading = False }
            , Cmd.none
            )

-- HTTP

loadUsers : Cmd Msg
loadUsers =
    Http.get
        { url = "http://localhost:5000/users"
        , expect = Http.expectString UserResult
        }

loadPosts : Cmd Msg
loadPosts =
    Http.get
        { url = "http://localhost:5000/posts"
        , expect = Http.expectString PostResult
        }

loadHealth : Cmd Msg
loadHealth =
    Http.get
        { url = "http://localhost:5000/health"
        , expect = Http.expectString HealthResult
        }

httpErrorToString : Http.Error -> String
httpErrorToString error =
    case error of
        Http.BadUrl url ->
            "Bad URL: " ++ url

        Http.Timeout ->
            "Request timeout"

        Http.NetworkError ->
            "Network error"

        Http.BadStatus status ->
            "Bad status: " ++ String.fromInt status

        Http.BadBody body ->
            "Bad body: " ++ body

-- VIEW

view : Model -> Html Msg
view model =
    div [ style "padding" "20px", style "font-family" "Arial, sans-serif" ]
        [ h1 [] [ text "ElmOpenApiClientGen Integration Test" ]
        , div [ style "margin-bottom" "20px" ]
            [ h2 [] [ text "Test Actions" ]
            , button 
                [ onClick TestGeneratedCode
                , style "padding" "10px 20px"
                , style "margin-right" "10px"
                , style "background-color" "#007cba"
                , style "color" "white"
                , style "border" "none"
                , style "border-radius" "4px"
                , style "cursor" "pointer"
                ]
                [ text "Test All Generated Code" ]
            , button 
                [ onClick LoadUsers
                , style "padding" "10px 20px"
                , style "margin-right" "10px"
                , style "background-color" "#28a745"
                , style "color" "white"
                , style "border" "none"
                , style "border-radius" "4px"
                , style "cursor" "pointer"
                ]
                [ text "Load Users" ]
            , button 
                [ onClick LoadPosts
                , style "padding" "10px 20px"
                , style "margin-right" "10px"
                , style "background-color" "#ffc107"
                , style "color" "black"
                , style "border" "none"
                , style "border-radius" "4px"
                , style "cursor" "pointer"
                ]
                [ text "Load Posts" ]
            , button 
                [ onClick LoadHealth
                , style "padding" "10px 20px"
                , style "background-color" "#17a2b8"
                , style "color" "white"
                , style "border" "none"
                , style "border-radius" "4px"
                , style "cursor" "pointer"
                ]
                [ text "Health Check" ]
            ]
        , if model.loading then
            div [ style "color" "#007cba" ] [ text "Loading..." ]
          else
            div []
                [ case model.error of
                    Just error ->
                        div [ style "color" "red", style "margin-bottom" "20px" ]
                            [ h3 [] [ text "Error:" ]
                            , text error
                            ]
                    Nothing ->
                        text ""
                , div [ style "margin-bottom" "20px" ]
                    [ h3 [] [ text "Generated Code Status" ]
                    , div [ style "padding" "10px", style "background-color" "#f8f9fa", style "border-radius" "4px" ]
                        [ text "This application will test the generated Elm code from ElmOpenApiClientGen"
                        , Html.br [] []
                        , text "Generated modules will be imported and used to make API calls"
                        ]
                    ]
                , viewResults "Users" model.users
                , viewResults "Posts" model.posts
                , case model.health of
                    Just health ->
                        div [ style "margin-bottom" "20px" ]
                            [ h3 [] [ text "Health Check" ]
                            , pre [ style "background-color" "#f8f9fa", style "padding" "10px", style "border-radius" "4px" ]
                                [ code [] [ text health ] ]
                            ]
                    Nothing ->
                        text ""
                ]
        ]

viewResults : String -> List String -> Html Msg
viewResults title results =
    if List.isEmpty results then
        text ""
    else
        div [ style "margin-bottom" "20px" ]
            [ h3 [] [ text title ]
            , ul []
                (List.map (\result ->
                    li []
                        [ pre [ style "background-color" "#f8f9fa", style "padding" "10px", style "border-radius" "4px" ]
                            [ code [] [ text result ] ]
                        ]
                ) results)
            ]

-- MAIN

main : Program () Model Msg
main =
    Browser.element
        { init = init
        , update = update
        , view = view
        , subscriptions = \_ -> Sub.none
        }