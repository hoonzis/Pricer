module.exports = {
    entry: {
        payoffCharts: "./CompiledJs/Pricer.Fabled/PayoffCharts",
        optionPrices: "./CompiledJs/Pricer.Fabled/OptionPrices",
        chartingTest: "./CompiledJs/Pricer.Fabled/ChartingTest"
    },
    output: {
        filename: "[name].bundle.js",
        path: "./out"
    },
    devtool: "source-map",
    module: {
        preLoaders: [{
            loader: "source-map-loader",
            exclude: /node_modules/,
        }],
    },
    externals: {
        "d3": "d3"
    }
};
