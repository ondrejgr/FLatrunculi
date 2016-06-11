module CoordTest

open Latrunculi.Model
open Latrunculi.Controller
open NUnit.Framework
open NUnit.Framework.Constraints

[<Test>]
let CoordTest() =


    let col = Coord.ColumnNumber 'A'
    Assert.IsTrue(match Coord.tryGetNextCol col with
                  | Success c -> c = Coord.ColumnNumber 'B'
                  | _ -> false)
    Assert.IsTrue(match Coord.tryGetPrevCol col with
                  | Error e -> true
                  | _ -> false)
    let col = Coord.ColumnNumber 'H'
    Assert.IsTrue(match Coord.tryGetNextCol col with
                  | Error e -> true
                  | _ -> false)
    Assert.IsTrue(match Coord.tryGetPrevCol col with
                  | Success c -> c = Coord.ColumnNumber 'G'
                  | _ -> false)
    let col = Coord.ColumnNumber 'D'
    Assert.IsTrue(match Coord.tryGetPrevCol col with
                  | Success c -> c = Coord.ColumnNumber 'C'
                  | _ -> false)
    Assert.IsTrue(match Coord.tryGetNextCol col with
                  | Success c -> c = Coord.ColumnNumber 'E'
                  | _ -> false)

    let row = Coord.RowNumber 7
    Assert.IsTrue(match Coord.tryGetNextRow row with
                  | Success c -> c = Coord.RowNumber 6
                  | _ -> false)
    Assert.IsTrue(match Coord.tryGetPrevRow row with
                  | Error e -> true
                  | _ -> false)
    let row = Coord.RowNumber 1
    Assert.IsTrue(match Coord.tryGetNextRow row with
                  | Error e -> true
                  | _ -> false)
    Assert.IsTrue(match Coord.tryGetPrevRow row with
                  | Success c -> c = Coord.RowNumber 2
                  | _ -> false)
    let row = Coord.RowNumber 4
    Assert.IsTrue(match Coord.tryGetPrevRow row with
                  | Success c -> c = Coord.RowNumber 5
                  | _ -> false)
    Assert.IsTrue(match Coord.tryGetNextRow row with
                  | Success c -> c = Coord.RowNumber 3
                  | _ -> false)

                  

    let coord = Coord.tryCreate 'F' 4
    Assert.IsTrue(match Coord.tryGetRelative coord Coord.Direction.Up with
                    | Success c -> Success c = Coord.tryCreate 'F' 5
                    | _ -> false)
    Assert.IsTrue(match Coord.tryGetRelative coord Coord.Direction.Down with
                    | Success c -> Success c = Coord.tryCreate 'F' 3
                    | _ -> false)
    Assert.IsTrue(match Coord.tryGetRelative coord Coord.Direction.Left with
                    | Success c -> Success c = Coord.tryCreate 'E' 4
                    | _ -> false)
    Assert.IsTrue(match Coord.tryGetRelative coord Coord.Direction.Right with
                    | Success c -> Success c = Coord.tryCreate 'G' 4
                    | _ -> false)
    let coord = Coord.tryCreate 'H' 1
    Assert.IsTrue(match Coord.tryGetRelative coord Coord.Direction.Right with
                    | Error e -> true
                    | _ -> false)
    Assert.IsTrue(match Coord.tryGetRelative coord Coord.Direction.Down with
                    | Error e -> true
                    | _ -> false)
    Assert.IsTrue(match Coord.tryGetRelative coord Coord.Direction.Up with
                    | Success c -> Success c = Coord.tryCreate 'H' 2
                    | _ -> false)
    Assert.IsTrue(match Coord.tryGetRelative coord Coord.Direction.Left with
                    | Success c -> Success c = Coord.tryCreate 'G' 1
                    | _ -> false)
    let coord = Coord.tryCreate 'A' 7
    Assert.IsTrue(match Coord.tryGetRelative coord Coord.Direction.Left with
                    | Error e -> true
                    | _ -> false)
    Assert.IsTrue(match Coord.tryGetRelative coord Coord.Direction.Up with
                    | Error e -> true
                    | _ -> false)
    Assert.IsTrue(match Coord.tryGetRelative coord Coord.Direction.Right with
                    | Success c -> Success c = Coord.tryCreate 'B' 7
                    | _ -> false)
    Assert.IsTrue(match Coord.tryGetRelative coord Coord.Direction.Down with
                    | Success c -> Success c = Coord.tryCreate 'A' 6
                    | _ -> false)

    ()