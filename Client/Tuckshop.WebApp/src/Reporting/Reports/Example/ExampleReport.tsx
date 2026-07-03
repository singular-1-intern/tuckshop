import { Neo } from "@singularsystems/neo-react";
import { NeoReport, Report } from "@singularsystems/neo-reporting";
import { ReportCategory } from "../ReportCategory";
import ExampleCriteria from "./ExampleCriteria";

@NeoReport("Example", { category: ReportCategory.Example })
export default class ExampleReport extends Report<ExampleCriteria> {

    constructor() {
        super(ExampleCriteria);
    }

    public renderCriteria() {
        return <div>
            <Neo.GridLayout md={2}>
                <Neo.FormGroup bind={this.criteria.meta.searchString} />
            </Neo.GridLayout>
        </div>;
    }
}