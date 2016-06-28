namespace Latrunculi.Controller
open Latrunculi.Model

module ReplayController =

    type T(model: ReplayModel.T) = 

        member private this.Model = model

    let tryCreate (model: ReplayModel.T): Result<T, Error> =
        maybe {
            return T(model) }
