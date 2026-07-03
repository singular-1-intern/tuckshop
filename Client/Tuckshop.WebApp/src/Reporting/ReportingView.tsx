import { ReportingViewBase, ReportingVMBase } from "@singularsystems/neo-reporting";
import { observer } from "mobx-react";

@observer
export default class ReportingView extends ReportingViewBase<ReportingVM> {

    constructor(props: unknown) {
        super(props, ReportingVM);
    }
}

class ReportingVM extends ReportingVMBase {

    constructor() {
        super();
        this.makeObservable();
    }
}