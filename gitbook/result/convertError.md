# Result.convertError

Namespace: `FsToolkit.ErrorHandling`

## Function Signature

```fsharp
Result<'a, 'error1> -> Result<'a, ^error2>
```

`^error2` is a [statically resolved parameter](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/generics/statically-resolved-type-parameters) with the below constraint:

```fsharp
^error2 : (static member From : 'error1 -> Result<'a, ^error2>)
```

This can be handy when working with result values whose types are parametrized with different error types if the overall result error value can be individually constructed from any of the intermediary error values. See the example below.

## Example

The following example is supposed to estimate the energy cost of climatizing a room. To do so, it must interact with a configuration file, temperature, humidity sensors, and a web service – any of these can potentially fail and come with custom error types.

`readConfigFile` may fail with a `ConfigFileErr` error value:

```fsharp
type ConfigFileErr = FileNotFound | InsufficientPermissions

let readConfigFile (filepath: string): Result<_, ConfigFileErr> =
  Ok {|DesiredTemperature = 20.0; DesiredHumidity = 50.0|}
```

`getTemperature` and `getHumidity` may fail with error values of type `TempSensorErr` and `HumiditySensorErr`, respectively:

```fsharp
type TempSensorErr = Disconnected | Uncalibrated
let getTemperature (): Result<_, TempSensorErr> = Ok 20.0

type HumiditySensorErr = DeviceTimedOut
let getHumidity (): Result<_, HumiditySensorErr> = Ok 60.0
```

`requestService` may fail with a `ServiceErr` error value:

```fsharp
type type ServiceErr = ServiceUnavailableErr

open System

let requestService (diffTemp: float) (diffHumidity: float): Result<_, ServiceErr> =
  Ok <| 20.5 * Math.Abs(diffTemp) + 51.1 * Math.Abs(diffHumidity) = ServiceUnavailableErr
```

We have an overall error type, `EstimationCostErr`, and a value of this can be created from any error value of type `ConfigFileErr`, `TempSensorErr`, `HumiditySensorErr`, or `ServiceErr`:

```fsharp
type EstimationCostErr =
| ServiceUnavailable
| ConfigFile of ConfigFileErr
| TempSensor of TempSensorErr
| HumiditySensor of HumiditySensorErr
| Service of ServiceErr
with
  static member From(err: ConfigFileErr) = ConfigFile err
  static member From(err: TempSensorErr) = TempSensor err
  static member From(err: HumiditySensorErr) = HumiditySensor err
  static member From(err: ServiceErr) = Service err
```

Finally, we can work with all result values inside the same [computation expression](../result/ce.md), even if they are parametrized with different error types, by piping the result value to `Result.convertError`:

```fsharp
let estimatedEnergyCost: Result<_, EstimationCostErr> = result {
  let! config = readConfigFile "myConfig.conf" |> Result.convertError
  and! temp = getTemperature() |> Result.convertError
  and! humidity = getHumidity() |> Result.convertError
  let tempDiff = temp - config.DesiredTemperature
  let humidityDiff = humidity - config.DesiredHumidity
  return! requestService tempDiff humidityDiff |> Result.convertError
}

printfn "%A" estimatedEnergyCost
// Ok 511.0
```
