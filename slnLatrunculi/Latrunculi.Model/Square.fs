﻿namespace Latrunculi.Model

module Square =

    type T =
        | Piece of Piece.T
        | Nothing

    let isEmpty x =
        match x with
        | Nothing -> true
        | _ -> false

    let createWithPiece piece =
        Piece piece

    let createEmpty =
        Nothing