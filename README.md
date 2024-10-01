# PSCircleTest

PS波描画の検証です。過程はこんな感じです https://x.com/ProjectS31415_1/status/1840678312143909194

Qiitaに記事を投稿しました。[P波・S波を簡単かつ正確に描画したい！(指定座標から指定した距離の円を描画する)](https://qiita.com/Ichihai1415/items/59c588ec5ce624f8b182)

既存コードコピペ等で汚いので使い物にならない気がしますが以下に注意して自由に使ってもらって大丈夫です。

## 権利表示

- 地図(Resourcesにある): 気象庁GISデータ AreaForecastLocalEを加工
- 2点の緯度経度から距離計算(CalDist.cs): https://qiita.com/matsuda_tkm/items/4eba5632535ca2f699b4 を移植
- 緯度経度と方位角と距離から緯度経度計算(CalLatLon.cs): https://qiita.com/r-fuji/items/5eefb451cf7113f1e51b を移植
- 深さと時間からPS波の距離計算(TimeTable.cs) https://zenn.dev/boocsan/articles/travel-time-table-converter-adcal2020 を参考に

- 移植にはCopilotを利用しました。
