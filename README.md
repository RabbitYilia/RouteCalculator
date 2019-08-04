# RouteCalculator

Calc supernet for routing by area

## DataSource

- Geolite2
- APNIC

## Result

You can get  CIDR without country in blacklist.

The less country in black the Outputs will be also less 

then you can add them to route table for something use just like below.

`route add {.CIDR} via {.OverSeaAccelerator}`

## Todo

- Add output file with multiple format.
- Add http proxy support
- Try to write in go

