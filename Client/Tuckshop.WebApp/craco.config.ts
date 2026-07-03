import { CracoConfig } from "@craco/types"

module.exports = {
  webpack: {
    configure: (webpackConfig, { env, paths }) => {
                
        webpackConfig.optimization = {
            splitChunks:
                env === "development" ? undefined : 
                {
                    chunks: "initial",
                    cacheGroups: {
                        neoSplit: {
                            /* 
                                Splits the node_modules (vendor) bundle into 2. 
                                One for neo packages, and one for the rest. 
                                Neo packages are likely to change more often than other packages.
                            */
                            test: /node_modules[\\/]/,
                            name(module: any) {
                                const matched = module.context.match(/[\\/]node_modules[\\/](.*?)([\\/]|$)/);
                                const packageName: string | undefined = matched && matched.length > 2 ? matched[1] : undefined;
        
                                if (packageName && packageName.startsWith("@singularsystems")) {
                                    return "singular-neo"
                                } else {
                                    return undefined;
                                }
                            },
                        },
                    }
                },
        }

        return webpackConfig;
      },
    }
} as CracoConfig;